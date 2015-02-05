using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.ComponentModel;

namespace Playground.Controls
{
    /// <summary>
    /// ListViewのGrid表示を模倣したコントロール.
    /// 固定列機能あり
    /// </summary>
    [TemplatePart(Name="PART_ScrollViewer", Type=typeof(ScrollViewer))]
    public class GridViewEx : ListBox
    {
        private ScrollViewer _scrollViewer = null;  // PART_ScrollViewer

        static GridViewEx()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(GridViewEx), new FrameworkPropertyMetadata(typeof(GridViewEx)));
        }

        #region FrozenColumns

        /// <summary>
        /// 固定する列のコレクション
        /// </summary>
        public GridViewColumnCollection FrozenColumns
        {
            get { return (GridViewColumnCollection)GetValue(FrozenColumnsProperty); }
            set { SetValue(FrozenColumnsProperty, value); }
        }

        public static readonly DependencyProperty FrozenColumnsProperty =
            DependencyProperty.Register("FrozenColumns", typeof(GridViewColumnCollection), typeof(GridViewEx), 
                                        new PropertyMetadata(new GridViewColumnCollection(), OnFrozenColumnsChanged));

        private static void OnFrozenColumnsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var target = d as GridViewEx;
            if (d == null)
                return;

            if (e.OldValue != null)
            {
                foreach (INotifyPropertyChanged col in (GridViewColumnCollection)e.OldValue)
                    col.PropertyChanged -= target.frozencol_PropertyChanged;
            }
            if (e.NewValue != null)
            {
                foreach (INotifyPropertyChanged col in (GridViewColumnCollection)e.NewValue)
                    col.PropertyChanged += target.frozencol_PropertyChanged;
            }
        }

        private void frozencol_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "ActualWidth")
            {
                CalculateOffsets();
            }
        }

        #endregion

        #region NormalColumns

        /// <summary>
        /// 固定しない(スクロールできる)列のコレクション
        /// </summary>
        public GridViewColumnCollection NormalColumns
        {
            get { return (GridViewColumnCollection)GetValue(NormalColumnsProperty); }
            set { SetValue(NormalColumnsProperty, value); }
        }

        public static readonly DependencyProperty NormalColumnsProperty =
            DependencyProperty.Register("NormalColumns", typeof(GridViewColumnCollection), typeof(GridViewEx), 
                                        new PropertyMetadata(new GridViewColumnCollection()));
        
        #endregion

        #region FrozenColumnsTotalWidth(ReadOnly)

        /// <summary>
        /// 固定列の合計幅
        /// </summary>
        public double FrozenColumnsTotalWidth
        {
            get { return (double)GetValue(FrozenColumnsTotalWidthProperty); }
            set { SetValue(FrozenColumnsTotalWidthPropertyKey, value); }
        }

        private static readonly DependencyPropertyKey FrozenColumnsTotalWidthPropertyKey =
            DependencyProperty.RegisterReadOnly("FrozenColumnsTotalWidth", typeof(double), typeof(GridViewEx), 
                                                new PropertyMetadata(0.0));
        public static readonly DependencyProperty FrozenColumnsTotalWidthProperty = FrozenColumnsTotalWidthPropertyKey.DependencyProperty;
        
        #endregion

        #region FrozenColumnsOffset (ReadOnly)

        /// <summary>
        /// 固定列を決まった位置に表示するための水平Offset値(For internal use)
        /// 普通は親のScrollViewerのHorizontalOffsetと一致する. (固定列の幅がScrollViewerの幅を超えている場合は調整が入る)
        /// </summary>
        public double FrozenColumnsOffset
        {
            get { return (double)GetValue(FrozenColumnsOffsetProperty); }
            protected set { SetValue(_FrozenColumnsOffsetPropertyKey, value); }
        }

        private static readonly DependencyPropertyKey _FrozenColumnsOffsetPropertyKey =
            DependencyProperty.RegisterReadOnly("FrozenColumnsOffset", typeof(double), typeof(GridViewEx),
                                                new PropertyMetadata(0.0));
        public static readonly DependencyProperty FrozenColumnsOffsetProperty = _FrozenColumnsOffsetPropertyKey.DependencyProperty;
        
        #endregion

        #region HorizontalScrollOffset (ReadOnly)
        
        public double HorizontalScrollOffset
        {
            get { return (double)GetValue(HorizontalScrollOffsetProperty); }
            protected set { SetValue(_HorizontalScrollOffsetPropertyKey, value); }
        }

        private static readonly DependencyPropertyKey _HorizontalScrollOffsetPropertyKey =
            DependencyProperty.RegisterReadOnly("HorizontalScrollOffset", typeof(double), typeof(GridViewEx),
                                                new PropertyMetadata(0.0));
        public static readonly DependencyProperty HorizontalScrollOffsetProperty = _HorizontalScrollOffsetPropertyKey.DependencyProperty;
        
        #endregion


        #region MinRowHeight
        
        /// <summary>
        /// 各行の高さの最小値
        /// </summary>
        /// <remarks>
        /// このプロパティを設定しておかないとVirtualizationが有効にならない(要調査)
        /// </remarks>
        public double MinRowHeight
        {
            get { return (double)GetValue(MinRowHeightProperty); }
            set { SetValue(MinRowHeightProperty, value); }
        }

        public static readonly DependencyProperty MinRowHeightProperty =
            DependencyProperty.Register("MinRowHeight", typeof(double), typeof(GridViewEx), new PropertyMetadata(22.0));

        #endregion
        
        private void CalculateOffsets()
        {
            var frozenWidth = this.FrozenColumns.Sum(col => col.ActualWidth + 1);
            this.FrozenColumnsTotalWidth = frozenWidth;
            if (_scrollViewer != null)
            {
                this.HorizontalScrollOffset = _scrollViewer.HorizontalOffset;
                var hOffset = _scrollViewer.HorizontalOffset;
                var viewWidth = _scrollViewer.ViewportWidth;
                if (frozenWidth < viewWidth)
                    this.FrozenColumnsOffset = hOffset;
                else if (hOffset + viewWidth - frozenWidth > 0)
                    this.FrozenColumnsOffset = hOffset + viewWidth - frozenWidth;
                else
                    this.FrozenColumnsOffset = 0;
            }
            else
            {
                this.HorizontalScrollOffset = 0;
                this.FrozenColumnsOffset = 0;
            }
        }
        
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            _scrollViewer = this.GetTemplateChild("PART_ScrollViewer") as ScrollViewer;
            if (_scrollViewer != null)
            {
                _scrollViewer.ScrollChanged += _scrollViewer_ScrollChanged;
                _scrollViewer.SizeChanged += _scrollViewer_SizeChanged;
            }
            this.CalculateOffsets();
        }

        private void _scrollViewer_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            this.CalculateOffsets();
        }

        void _scrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            this.CalculateOffsets();
        }

        protected override DependencyObject GetContainerForItemOverride()
        {
            // Virtualizationを有効にするために明示的にMinHeightを設定する
            // (xamlでBindingしただけではVirtualizationがうまく動かなかった。要調査)
            var container = (ListBoxItem)base.GetContainerForItemOverride();
            container.MinHeight = this.MinRowHeight;
            return container;
        }
    }
}
