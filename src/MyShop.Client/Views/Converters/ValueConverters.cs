using System;
using System.Collections;
using System.Linq;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;

namespace MyShop.Client.Views.Converters {
    /// <summary>
    /// Converter chuyển đổi string thành Visibility.
    /// Trả về Visible nếu string không null hoặc rỗng, ngược lại trả về Collapsed.
    /// </summary>
    /// <remarks>
    /// Converter này hữu ích để hiển thị/ẩn các control dựa trên việc có nội dung string hay không.
    /// Thường được sử dụng để hiển thị thông báo lỗi hoặc label khi có dữ liệu.
    /// </remarks>
    public class StringToVisibilityConverter : IValueConverter {
        /// <summary>
        /// Chuyển đổi giá trị string thành Visibility.
        /// </summary>
        /// <param name="value">Giá trị string cần chuyển đổi</param>
        /// <param name="targetType">Loại đích của binding (thường là Visibility)</param>
        /// <param name="parameter">Tham số converter (không được sử dụng)</param>
        /// <param name="language">Ngôn ngữ culture (không được sử dụng)</param>
        /// <returns>
        /// Visibility.Visible nếu string không null và không rỗng,
        /// ngược lại trả về Visibility.Collapsed
        /// </returns>
        public object Convert(object value, Type targetType, object parameter, string language) {
            return string.IsNullOrEmpty(value as string) ? Visibility.Collapsed : Visibility.Visible;
        }

        /// <summary>
        /// Chuyển đổi ngược từ Visibility về string.
        /// Phương thức này không được hỗ trợ và sẽ throw NotImplementedException.
        /// </summary>
        /// <param name="value">Giá trị Visibility</param>
        /// <param name="targetType">Loại đích của binding</param>
        /// <param name="parameter">Tham số converter</param>
        /// <param name="language">Ngôn ngữ culture</param>
        /// <returns>Không trả về giá trị nào</returns>
        /// <exception cref="NotImplementedException">Luôn được throw vì conversion ngược không được hỗ trợ</exception>
        public object ConvertBack(object value, Type targetType, object parameter, string language) {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Converter đảo ngược giá trị boolean.
    /// Chuyển đổi true thành false và ngược lại.
    /// </summary>
    /// <remarks>
    /// Converter này hữu ích khi cần đảo ngược logic boolean trong binding.
    /// Thường được sử dụng để disable button khi IsLoading = true.
    /// </remarks>
    public class BoolNegationConverter : IValueConverter {
        /// <summary>
        /// Đảo ngược giá trị boolean.
        /// </summary>
        /// <param name="value">Giá trị boolean cần đảo ngược</param>
        /// <param name="targetType">Loại đích của binding (thường là bool)</param>
        /// <param name="parameter">Tham số converter (không được sử dụng)</param>
        /// <param name="language">Ngôn ngữ culture (không được sử dụng)</param>
        /// <returns>Giá trị boolean đã được đảo ngược</returns>
        public object Convert(object value, Type targetType, object parameter, string language) {
            return !(bool)value;
        }

        /// <summary>
        /// Đảo ngược giá trị boolean trong conversion ngược.
        /// </summary>
        /// <param name="value">Giá trị boolean cần đảo ngược</param>
        /// <param name="targetType">Loại đích của binding</param>
        /// <param name="parameter">Tham số converter (không được sử dụng)</param>
        /// <param name="language">Ngôn ngữ culture (không được sử dụng)</param>
        /// <returns>Giá trị boolean đã được đảo ngược</returns>
        public object ConvertBack(object value, Type targetType, object parameter, string language) {
            return !(bool)value;
        }
    }

    /// <summary>
    /// Converter chuyển đổi boolean thành Visibility.
    /// Trả về Visible nếu true, Collapsed nếu false.
    /// </summary>
    /// <remarks>
    /// Converter này là một trong những converter phổ biến nhất trong WPF/WinUI.
    /// Được sử dụng để hiển thị/ẩn control dựa trên điều kiện boolean.
    /// </remarks>
    public class BoolToVisibilityConverter : IValueConverter {
        /// <summary>
        /// Chuyển đổi giá trị boolean thành Visibility.
        /// </summary>
        /// <param name="value">Giá trị boolean cần chuyển đổi</param>
        /// <param name="targetType">Loại đích của binding (thường là Visibility)</param>
        /// <param name="parameter">Tham số converter (không được sử dụng)</param>
        /// <param name="language">Ngôn ngữ culture (không được sử dụng)</param>
        /// <returns>
        /// Visibility.Visible nếu giá trị là true,
        /// Visibility.Collapsed nếu giá trị là false
        /// </returns>
        public object Convert(object value, Type targetType, object parameter, string language) {
            return (bool)value ? Visibility.Visible : Visibility.Collapsed;
        }

        /// <summary>
        /// Chuyển đổi ngược từ Visibility về boolean.
        /// </summary>
        /// <param name="value">Giá trị Visibility cần chuyển đổi</param>
        /// <param name="targetType">Loại đích của binding (thường là bool)</param>
        /// <param name="parameter">Tham số converter (không được sử dụng)</param>
        /// <param name="language">Ngôn ngữ culture (không được sử dụng)</param>
        /// <returns>
        /// true nếu Visibility là Visible,
        /// false nếu Visibility là Collapsed hoặc Hidden
        /// </returns>
        public object ConvertBack(object value, Type targetType, object parameter, string language) {
            return (Visibility)value == Visibility.Visible;
        }
    }

    /// <summary>
    /// Converter chuyển đổi danh sách validation errors thành string lỗi đầu tiên.
    /// Được sử dụng để hiển thị thông báo lỗi validation trong UI.
    /// </summary>
    /// <remarks>
    /// Converter này làm việc với validation framework để hiển thị thông báo lỗi.
    /// Nó lấy lỗi đầu tiên từ danh sách và hiển thị dưới dạng string.
    /// Nếu không có lỗi nào, trả về chuỗi rỗng.
    /// </remarks>
    public class ValidationErrorConverter : IValueConverter {
        /// <summary>
        /// Chuyển đổi collection validation errors thành string thông báo lỗi đầu tiên.
        /// </summary>
        /// <param name="value">Collection chứa các validation error</param>
        /// <param name="targetType">Loại đích của binding (thường là string)</param>
        /// <param name="parameter">Tham số converter (không được sử dụng)</param>
        /// <param name="language">Ngôn ngữ culture (không được sử dụng)</param>
        /// <returns>
        /// String chứa thông báo lỗi đầu tiên nếu có,
        /// ngược lại trả về chuỗi rỗng
        /// </returns>
        public object Convert(object value, Type targetType, object parameter, string language) {
            if (value is IEnumerable errors) {
                var errorList = errors.Cast<object>().ToList();
                return errorList.FirstOrDefault()?.ToString() ?? string.Empty;
            }
            return string.Empty;
        }

        /// <summary>
        /// Chuyển đổi ngược từ string về validation errors.
        /// Phương thức này không được hỗ trợ và sẽ throw NotImplementedException.
        /// </summary>
        /// <param name="value">String thông báo lỗi</param>
        /// <param name="targetType">Loại đích của binding</param>
        /// <param name="parameter">Tham số converter</param>
        /// <param name="language">Ngôn ngữ culture</param>
        /// <returns>Không trả về giá trị nào</returns>
        /// <exception cref="NotImplementedException">Luôn được throw vì conversion ngược không được hỗ trợ</exception>
        public object ConvertBack(object value, Type targetType, object parameter, string language) {
            throw new NotImplementedException();
        }
    }
}