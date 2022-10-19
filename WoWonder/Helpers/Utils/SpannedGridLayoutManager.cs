using System;
using System.Collections.Generic;
using Android.Content;
using Android.Content.Res;
using Android.Graphics;
using Android.Runtime;
using Android.Util;
using Android.Views;
using AndroidX.RecyclerView.Widget;

namespace WoWonder.Helpers.Utils
{
    public class SpannedGridLayoutManager : RecyclerView.LayoutManager
    {

        private IGridSpanLookup MSpanLookup;
        private int Columns = 1;
        private float CellAspectRatio = 1f;

        private int CellHeight;
        private int[] CellBorders;
        private int FirstVisiblePosition;
        private int LastVisiblePosition;
        private int FirstVisibleRow;
        private int LastVisibleRow;
        private bool ForceClearOffsets;
        private SparseArray<GridCell> Cells;
        private List<int> FirstChildPositionForRow; // key == row, val == first child position
        private int TotalRows;
        private readonly Rect ItemDecorationInsets = new Rect();

        public SpannedGridLayoutManager(IGridSpanLookup spanLookup, int columns, float cellAspectRatio)
        {
            try
            {
                MSpanLookup = spanLookup;
                Columns = columns;
                CellAspectRatio = cellAspectRatio;
                AutoMeasureEnabled = true;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public SpannedGridLayoutManager(Context context, IAttributeSet attrs, int defStyleAttr, int defStyleRes)
        {
            try
            {
                TypedArray a = context.ObtainStyledAttributes(attrs, Resource.Styleable.SpannedGridLayoutManager, defStyleAttr, defStyleRes);
                Columns = a.GetInt(Resource.Styleable.SpannedGridLayoutManager_spanCount, 1);
                ParseAspectRatio(a.GetString(Resource.Styleable.SpannedGridLayoutManager_aspectRatio));
                // TODO use this!
                int orientation = a.GetInt(Resource.Styleable.SpannedGridLayoutManager_android_orientation, RecyclerView.Vertical);
                a.Recycle();
                AutoMeasureEnabled = true;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public interface IGridSpanLookup
        {
            SpanInfo GetSpanInfo(int position);
        }

        public virtual IGridSpanLookup SpanLookup
        {
            set
            {
                MSpanLookup = value;
            }
        }

        public class SpanInfo
        {
            public int ColumnSpan;
            public int RowSpan;

            public SpanInfo(int columnSpan, int rowSpan)
            {
                ColumnSpan = columnSpan;
                RowSpan = rowSpan;
            }

            public static readonly SpanInfo SingleCell = new SpanInfo(1, 1);
        }

        public class LayoutParams : RecyclerView.LayoutParams
        {

            internal int ColumnSpan;
            internal int RowSpan;

            protected LayoutParams(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
            {
            }

            public LayoutParams(Context c, IAttributeSet attrs) : base(c, attrs)
            {
            }

            public LayoutParams(ViewGroup.LayoutParams source) : base(source)
            {
            }

            public LayoutParams(ViewGroup.MarginLayoutParams source) : base(source)
            {
            }

            public LayoutParams(RecyclerView.LayoutParams source) : base(source)
            {
            }

            public LayoutParams(int width, int height) : base(width, height)
            {
            }
        }

        public override void OnLayoutChildren(RecyclerView.Recycler recycler, RecyclerView.State state)
        {
            try
            {
                CalculateWindowSize();
                CalculateCellPositions(recycler, state);

                if (state.ItemCount == 0)
                {
                    DetachAndScrapAttachedViews(recycler);
                    FirstVisibleRow = 0;
                    ResetVisibleItemTracking();
                    return;
                }

                // TODO use orientationHelper
                int startTop = PaddingTop;
                int scrollOffset = 0;
                if (ForceClearOffsets)
                { // see #scrollToPosition
                    startTop = -(FirstVisibleRow * CellHeight);
                    ForceClearOffsets = false;
                }
                else if (ChildCount != 0)
                {
                    scrollOffset = GetDecoratedTop(GetChildAt(0));
                    startTop = scrollOffset - FirstVisibleRow * CellHeight;
                    ResetVisibleItemTracking();
                }

                DetachAndScrapAttachedViews(recycler);
                int row = FirstVisibleRow;
                int availableSpace = Height - scrollOffset;
                int lastItemPosition = state.ItemCount - 1;
                //while (availableSpace > 0 && LastVisiblePosition < lastItemPosition)
                //{
                //    availableSpace -= LayoutRow(row, startTop, recycler, state);
                //    row = GetNextSpannedRow(row);
                //}

                //wael
                while (LastVisiblePosition < lastItemPosition)
                {
                    availableSpace -= LayoutRow(row, startTop, recycler, state);
                    row = GetNextSpannedRow(row);
                }

                LayoutDisappearingViews(recycler, state, startTop);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public override RecyclerView.LayoutParams GenerateDefaultLayoutParams()
        {
            return new LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent);
        }

        public override RecyclerView.LayoutParams GenerateLayoutParams(Context c, IAttributeSet attrs)
        {
            return new LayoutParams(c, attrs);
        }

        public override RecyclerView.LayoutParams GenerateLayoutParams(ViewGroup.LayoutParams lp)
        {
            if (lp is ViewGroup.MarginLayoutParams)
            {
                return new LayoutParams((ViewGroup.MarginLayoutParams)lp);
            }
            else
            {
                return new LayoutParams(lp);
            }
        }

        public override bool CheckLayoutParams(RecyclerView.LayoutParams lp)
        {
            return lp is LayoutParams;
        }

        public override void OnAdapterChanged(RecyclerView.Adapter oldAdapter, RecyclerView.Adapter newAdapter)
        {
            try
            {
                RemoveAllViews();
                Reset();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public override bool SupportsPredictiveItemAnimations()
        {
            return true;
        }

        public override bool CanScrollVertically()
        {
            return true;
        }

        public override int ScrollVerticallyBy(int dy, RecyclerView.Recycler recycler, RecyclerView.State state)
        {
            try
            {
                if (ChildCount == 0 || dy == 0)
                {
                    return 0;
                }

                int scrolled;
                int top = GetDecoratedTop(GetChildAt(0));

                if (dy < 0)
                { // scrolling content down
                    if (FirstVisibleRow == 0)
                    { // at top of content
                        int scrollRange = -(PaddingTop - top);
                        scrolled = Math.Max(dy, scrollRange);
                    }
                    else
                    {
                        scrolled = dy;
                    }
                    if (top - scrolled >= 0)
                    { // new top row came on screen
                        int newRow = FirstVisibleRow - 1;
                        if (newRow >= 0)
                        {
                            int startOffset = top - FirstVisibleRow * CellHeight;
                            LayoutRow(newRow, startOffset, recycler, state);
                        }
                    }
                    int firstPositionOfLastRow = GetFirstPositionInSpannedRow(LastVisibleRow);
                    int lastRowTop = GetDecoratedTop(GetChildAt(firstPositionOfLastRow - FirstVisiblePosition));
                    if (lastRowTop - scrolled > Height)
                    { // last spanned row scrolled out
                        RecycleRow(LastVisibleRow, recycler, state);
                    }
                }
                else
                { // scrolling content up
                    int bottom = GetDecoratedBottom(GetChildAt(ChildCount - 1));
                    if (LastVisiblePosition == ItemCount - 1)
                    { // is at end of content
                        int scrollRange = Math.Max(bottom - Height + PaddingBottom, 0);
                        scrolled = Math.Min(dy, scrollRange);
                    }
                    else
                    {
                        scrolled = dy;
                    }
                    if (bottom - scrolled < Height)
                    { // new row scrolled in
                        int nextRow = LastVisibleRow + 1;
                        if (nextRow < SpannedRowCount)
                        {
                            int startOffset = top - FirstVisibleRow * CellHeight;
                            LayoutRow(nextRow, startOffset, recycler, state);
                        }
                    }
                    int lastPositionInRow = GetLastPositionInSpannedRow(FirstVisibleRow, state);
                    int bottomOfFirstRow = GetDecoratedBottom(GetChildAt(lastPositionInRow - FirstVisiblePosition));
                    if (bottomOfFirstRow - scrolled < 0)
                    { // first spanned row scrolled out
                        RecycleRow(FirstVisibleRow, recycler, state);
                    }
                }
                OffsetChildrenVertical(-scrolled);
                return scrolled;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return 0;
            }
        }

        public override void ScrollToPosition(int position)
        {
            try
            {
                if (position >= ItemCount)
                {
                    position = ItemCount - 1;
                }

                FirstVisibleRow = GetRowIndex(position);
                ResetVisibleItemTracking();
                ForceClearOffsets = true;
                RemoveAllViews();
                RequestLayout();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public override void SmoothScrollToPosition(RecyclerView recyclerView, RecyclerView.State state, int position)
        {
            try
            {
                if (position >= ItemCount)
                {
                    position = ItemCount - 1;
                }

                LinearSmoothScroller scroller = new LinearSmoothScrollerAnonymousInnerClass(this, recyclerView.Context);
                scroller.TargetPosition = position;
                StartSmoothScroll(scroller);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private class LinearSmoothScrollerAnonymousInnerClass : LinearSmoothScroller
        {
            private readonly SpannedGridLayoutManager OuterInstance;

            public LinearSmoothScrollerAnonymousInnerClass(SpannedGridLayoutManager outerInstance, Context context) : base(context)
            {
                OuterInstance = outerInstance;
            }

            public override PointF ComputeScrollVectorForPosition(int targetPosition)
            {
                int rowOffset = OuterInstance.GetRowIndex(targetPosition) - OuterInstance.FirstVisibleRow;
                return new PointF(0, rowOffset * OuterInstance.CellHeight);
            }
        }

        public override int ComputeVerticalScrollRange(RecyclerView.State state)
        {
            // TODO update this to incrementally calculate
            return SpannedRowCount * CellHeight + PaddingTop + PaddingBottom;
        }

        public override int ComputeVerticalScrollExtent(RecyclerView.State state)
        {
            return Height;
        }

        public override int ComputeVerticalScrollOffset(RecyclerView.State state)
        {
            if (ChildCount == 0)
            {
                return 0;
            }
            return PaddingTop + FirstVisibleRow * CellHeight - GetDecoratedTop(GetChildAt(0));
        }

        public override View FindViewByPosition(int position)
        {
            if (position < FirstVisiblePosition || position > LastVisiblePosition)
            {
                return null;
            }
            return GetChildAt(position - FirstVisiblePosition);
        }

        public virtual int FirstVisibleItemPosition
        {
            get
            {
                return FirstVisiblePosition;
            }
        }

        private class GridCell
        {
            internal readonly int Row;
            internal readonly int RowSpan;
            internal readonly int Column;
            internal readonly int ColumnSpan;

            internal GridCell(int row, int rowSpan, int column, int columnSpan)
            {
                try
                {
                    Row = row;
                    RowSpan = rowSpan;
                    Column = column;
                    ColumnSpan = columnSpan;
                }
                catch (Exception e)
                {
                    Methods.DisplayReportResultTrack(e);
                }
            }
        }

        /// <summary>
        /// This is the main layout algorithm, iterates over all items and places them into [column, row]
        /// cell positions. Stores this layout info for use later on. Also records the adapter position
        /// that each row starts at.
        /// <para>
        /// Note that if a row is spanned, then the row start position is recorded as the first cell of
        /// the row that the spanned cell starts in. This is to ensure that we have sufficient contiguous
        /// views to layout/draw a spanned row.
        /// </para>
        /// </summary>
        private void CalculateCellPositions(RecyclerView.Recycler recycler, RecyclerView.State state)
        {
            try
            {
                int itemCount = state.ItemCount;
                Cells = new SparseArray<GridCell>(itemCount);
                FirstChildPositionForRow = new List<int>();
                int row = 0;
                int column = 0;
                RecordSpannedRowStartPosition(row, column);
                int[] rowHwm = new int[Columns]; // row high water mark (per column)
                for (int position = 0; position < itemCount; position++)
                {

                    SpanInfo spanInfo;
                    int adapterPosition = recycler.ConvertPreLayoutPositionToPostLayout(position);
                    if (adapterPosition != RecyclerView.NoPosition)
                    {
                        spanInfo = MSpanLookup.GetSpanInfo(adapterPosition);
                    }
                    else
                    {
                        // item removed from adapter, retrieve its previous span info
                        // as we can't get from the lookup (adapter)
                        spanInfo = GetSpanInfoFromAttachedView(position);
                    }

                    if (spanInfo.ColumnSpan > Columns)
                    {
                        spanInfo.ColumnSpan = Columns; // or should we throw?
                    }

                    // check horizontal space at current position else start a new row
                    // note that this may leave gaps in the grid; we don't backtrack to try and fit
                    // subsequent cells into gaps. We place the responsibility on the adapter to provide
                    // continuous data i.e. that would not span column boundaries to avoid gaps.
                    if (column + spanInfo.ColumnSpan > Columns)
                    {
                        row++;
                        RecordSpannedRowStartPosition(row, position);
                        column = 0;
                    }

                    // check if this cell is already filled (by previous spanning cell)
                    while (rowHwm[column] > row)
                    {
                        column++;
                        if (column + spanInfo.ColumnSpan > Columns)
                        {
                            row++;
                            RecordSpannedRowStartPosition(row, position);
                            column = 0;
                        }
                    }

                    // by this point, cell should fit at [column, row]
                    Cells.Put(position, new GridCell(row, spanInfo.RowSpan, column, spanInfo.ColumnSpan));

                    // update the high water mark book-keeping
                    for (int columnsSpanned = 0; columnsSpanned < spanInfo.ColumnSpan; columnsSpanned++)
                    {
                        rowHwm[column + columnsSpanned] = row + spanInfo.RowSpan;
                    }

                    // if we're spanning rows then record the 'first child position' as the first item
                    // *in the row the spanned item starts*. i.e. the position might not actually sit
                    // within the row but it is the earliest position we need to render in order to fill
                    // the requested row.
                    if (spanInfo.RowSpan > 1)
                    {
                        int rowStartPosition = GetFirstPositionInSpannedRow(row);
                        for (int rowsSpanned = 1; rowsSpanned < spanInfo.RowSpan; rowsSpanned++)
                        {
                            int spannedRow = row + rowsSpanned;
                            RecordSpannedRowStartPosition(spannedRow, rowStartPosition);
                        }
                    }

                    // increment the current position
                    column += spanInfo.ColumnSpan;
                }
                TotalRows = rowHwm[0];
                for (int i = 1; i < rowHwm.Length; i++)
                {
                    if (rowHwm[i] > TotalRows)
                    {
                        TotalRows = rowHwm[i];
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private SpanInfo GetSpanInfoFromAttachedView(int position)
        {
            try
            {
                for (int i = 0; i < ChildCount; i++)
                {
                    View child = GetChildAt(i);
                    if (position == GetPosition(child))
                    {
                        LayoutParams lp = (LayoutParams)child.LayoutParameters;
                        return new SpanInfo(lp.ColumnSpan, lp.RowSpan);
                    }
                }
                // errrrr?
                return SpanInfo.SingleCell;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return SpanInfo.SingleCell;
            }
        }

        private void RecordSpannedRowStartPosition(int rowIndex, int position)
        {
            try
            {
                if (SpannedRowCount < rowIndex + 1)
                {
                    FirstChildPositionForRow.Add(position);
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private int GetRowIndex(int position)
        {
            return position < Cells.Size() ? Cells.Get(position).Row : -1;
        }

        private int SpannedRowCount
        {
            get
            {
                return FirstChildPositionForRow.Count;
            }
        }

        private int GetNextSpannedRow(int rowIndex)
        {
            try
            {
                int firstPositionInRow = GetFirstPositionInSpannedRow(rowIndex);
                int nextRow = rowIndex + 1;
                while (nextRow < SpannedRowCount && GetFirstPositionInSpannedRow(nextRow) == firstPositionInRow)
                {
                    nextRow++;
                }
                return nextRow;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return 0;
            }
        }

        private int GetFirstPositionInSpannedRow(int rowIndex)
        {
            return FirstChildPositionForRow[rowIndex];
        }

        private int GetLastPositionInSpannedRow(int rowIndex, RecyclerView.State state)
        {
            int nextRow = GetNextSpannedRow(rowIndex);
            return nextRow != SpannedRowCount ? GetFirstPositionInSpannedRow(nextRow) - 1 : state.ItemCount - 1;
        }

        /// <summary>
        /// Lay out a given 'row'. We might actually add more that one row if the requested row contains
        /// a row-spanning cell. Returns the pixel height of the rows laid out.
        /// <para>
        /// To simplify logic & book-keeping, views are attached in adapter order, that is child 0 will
        /// always be the earliest position displayed etc.
        /// </para>
        /// </summary>
        private int LayoutRow(int rowIndex, int startTop, RecyclerView.Recycler recycler, RecyclerView.State state)
        {
            try
            {
                int firstPositionInRow = GetFirstPositionInSpannedRow(rowIndex);
                int lastPositionInRow = GetLastPositionInSpannedRow(rowIndex, state);
                bool containsRemovedItems = false;

                int insertPosition = rowIndex < FirstVisibleRow ? 0 : ChildCount;
                for (int position = firstPositionInRow; position <= lastPositionInRow; position++, insertPosition++)
                {

                    View view = recycler.GetViewForPosition(position);
                    LayoutParams lp = (LayoutParams)view.LayoutParameters;
                    containsRemovedItems |= lp.IsItemRemoved;
                    GridCell cell = Cells.Get(position);
                    AddView(view, insertPosition);

                    // TODO use orientation helper
                    int wSpec = GetChildMeasureSpec(CellBorders[cell.Column + cell.ColumnSpan] - CellBorders[cell.Column], (int)MeasureSpecMode.Exactly, 0, lp.Width, false);
                    int hSpec = GetChildMeasureSpec(cell.RowSpan * CellHeight, (int)MeasureSpecMode.Exactly, 0, lp.Height, true);
                    MeasureChildWithDecorationsAndMargin(view, wSpec, hSpec);

                    int left = CellBorders[cell.Column] + lp.LeftMargin;
                    int top = startTop + cell.Row * CellHeight + lp.TopMargin;
                    int right = left + GetDecoratedMeasuredWidth(view);
                    int bottom = top + GetDecoratedMeasuredHeight(view);
                    LayoutDecorated(view, left, top, right, bottom);
                    lp.ColumnSpan = cell.ColumnSpan;
                    lp.RowSpan = cell.RowSpan;
                }

                if (firstPositionInRow < FirstVisiblePosition)
                {
                    FirstVisiblePosition = firstPositionInRow;
                    FirstVisibleRow = GetRowIndex(FirstVisiblePosition);
                }
                if (lastPositionInRow > LastVisiblePosition)
                {
                    LastVisiblePosition = lastPositionInRow;
                    LastVisibleRow = GetRowIndex(LastVisiblePosition);
                }
                if (containsRemovedItems)
                {
                    return 0; // don't consume space for rows with disappearing items
                }

                GridCell first = Cells.Get(firstPositionInRow);
                GridCell last = Cells.Get(lastPositionInRow);
                return (last.Row + last.RowSpan - first.Row) * CellHeight;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return 0;
            }
        }

        /// <summary>
        /// Remove and recycle all items in this 'row'. If the row includes a row-spanning cell then all
        /// cells in the spanned rows will be removed.
        /// </summary>
        private void RecycleRow(int rowIndex, RecyclerView.Recycler recycler, RecyclerView.State state)
        {
            try
            {
                int firstPositionInRow = GetFirstPositionInSpannedRow(rowIndex);
                int lastPositionInRow = GetLastPositionInSpannedRow(rowIndex, state);
                int toRemove = lastPositionInRow;
                while (toRemove >= firstPositionInRow)
                {
                    int index = toRemove - FirstVisiblePosition;
                    RemoveAndRecycleViewAt(index, recycler);
                    toRemove--;
                }
                if (rowIndex == FirstVisibleRow)
                {
                    FirstVisiblePosition = lastPositionInRow + 1;
                    FirstVisibleRow = GetRowIndex(FirstVisiblePosition);
                }
                if (rowIndex == LastVisibleRow)
                {
                    LastVisiblePosition = firstPositionInRow - 1;
                    LastVisibleRow = GetRowIndex(LastVisiblePosition);
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void LayoutDisappearingViews(RecyclerView.Recycler recycler, RecyclerView.State state, int startTop)
        {
            // TODO
        }

        private void CalculateWindowSize()
        {
            try
            {
                // TODO use OrientationHelper.getTotalSpace
                int cellWidth = (int)Math.Floor((double)((Width - PaddingLeft - PaddingRight) / Columns));
                CellHeight = (int)Math.Floor(cellWidth * (1f / CellAspectRatio));
                CalculateCellBorders();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void Reset()
        {
            try
            {
                Cells = null;
                FirstChildPositionForRow = null;
                FirstVisiblePosition = 0;
                FirstVisibleRow = 0;
                LastVisiblePosition = 0;
                LastVisibleRow = 0;
                CellHeight = 0;
                ForceClearOffsets = false;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void ResetVisibleItemTracking()
        {
            try
            {
                // maintain the firstVisibleRow but reset other state vars
                // TODO make orientation agnostic
                int minimumVisibleRow = MinimumFirstVisibleRow;
                if (FirstVisibleRow > minimumVisibleRow)
                {
                    FirstVisibleRow = minimumVisibleRow;
                }
                FirstVisiblePosition = GetFirstPositionInSpannedRow(FirstVisibleRow);
                LastVisibleRow = FirstVisibleRow;
                LastVisiblePosition = FirstVisiblePosition;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private int MinimumFirstVisibleRow
        {
            get
            {
                int maxDisplayedRows = (int)Math.Ceiling((float)Height / CellHeight) + 1;
                if (TotalRows < maxDisplayedRows)
                {
                    return 0;
                }
                int minFirstRow = TotalRows - maxDisplayedRows;
                // adjust to spanned rows
                return GetRowIndex(GetFirstPositionInSpannedRow(minFirstRow));
            }
        }

        /* Adapted from GridLayoutManager */
        private void CalculateCellBorders()
        {
            try
            {
                CellBorders = new int[Columns + 1];
                int totalSpace = Width - PaddingLeft - PaddingRight;
                int consumedPixels = PaddingLeft;
                CellBorders[0] = consumedPixels;
                int sizePerSpan = totalSpace / Columns;
                int sizePerSpanRemainder = totalSpace % Columns;
                int additionalSize = 0;
                for (int i = 1; i <= Columns; i++)
                {
                    int itemSize = sizePerSpan;
                    additionalSize += sizePerSpanRemainder;
                    if (additionalSize > 0 && Columns - additionalSize < sizePerSpanRemainder)
                    {
                        itemSize += 1;
                        additionalSize -= Columns;
                    }
                    consumedPixels += itemSize;
                    CellBorders[i] = consumedPixels;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void MeasureChildWithDecorationsAndMargin(View child, int widthSpec, int heightSpec)
        {
            try
            {
                CalculateItemDecorationsForChild(child, ItemDecorationInsets);
                RecyclerView.LayoutParams lp = (RecyclerView.LayoutParams)child.LayoutParameters;
                widthSpec = UpdateSpecWithExtra(widthSpec, lp.LeftMargin + ItemDecorationInsets.Left, lp.RightMargin + ItemDecorationInsets.Right);
                heightSpec = UpdateSpecWithExtra(heightSpec, lp.TopMargin + ItemDecorationInsets.Top, lp.BottomMargin + ItemDecorationInsets.Bottom);
                child.Measure(widthSpec, heightSpec);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private int UpdateSpecWithExtra(int spec, int startInset, int endInset)
        {
            try
            {
                if (startInset == 0 && endInset == 0)
                {
                    return spec;
                }
                MeasureSpecMode mode = View.MeasureSpec.GetMode(spec);
                if (mode == MeasureSpecMode.AtMost || mode == MeasureSpecMode.Exactly)
                {
                    return View.MeasureSpec.MakeMeasureSpec(View.MeasureSpec.GetSize(spec) - startInset - endInset, mode);
                }
                return spec;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return spec;
            }
        }

        /* Adapted from ConstraintLayout */
        private void ParseAspectRatio(string aspect)
        {
            try
            {
                if (!ReferenceEquals(aspect, null))
                {
                    int colonIndex = aspect.IndexOf(':');
                    if (colonIndex >= 0 && colonIndex < aspect.Length - 1)
                    {
                        string nominator = aspect.Substring(0, colonIndex);
                        string denominator = aspect.Substring(colonIndex + 1);
                        if (nominator.Length > 0 && denominator.Length > 0)
                        {
                            try
                            {
                                float nominatorValue = float.Parse(nominator);
                                float denominatorValue = float.Parse(denominator);
                                if (nominatorValue > 0 && denominatorValue > 0)
                                {
                                    CellAspectRatio = Math.Abs(nominatorValue / denominatorValue);
                                    return;
                                }
                            }
                            catch (FormatException)
                            {
                                // Ignore
                            }
                        }
                    }
                }
                throw new ArgumentException("Could not parse aspect ratio: '" + aspect + "'");
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

    }

}