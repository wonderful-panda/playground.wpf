using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using IronPython.Hosting;
using Microsoft.Scripting.Hosting;

namespace Radish
{
    /// <summary>
    /// Dynamic converter powered by IronPython
    /// </summary>
    public class PythonConverter : IValueConverter, IMultiValueConverter
    {
        readonly ScriptEngine _engine;
        readonly ScriptScope _scope;

        readonly Dictionary<string, Func<object, object>> _functions = new Dictionary<string, Func<object, object>>();

        public PythonConverter(string[] assemblies, string[] modules)
        {
            _engine = Python.CreateEngine();
            _scope = _engine.CreateScope();
            this.AddReferences(assemblies);
            this.ImportModule(modules);
        }

        public void AddReferences(params string[] assemblies)
        {
            var script = new StringBuilder();
            script.AppendLine("import clr");
            foreach (var assembly in assemblies)
                script.AppendFormat("clr.AddReference('{0}')", assembly).AppendLine();
            _engine.Execute(script.ToString());
        }

        public void ImportModule(params string[] modules)
        {
            foreach (var module in modules)
                _engine.Execute("import " + module, _scope);
        }

        public Func<object, object> DefineFunction(string source)
        {
            Func<object, object> func;
            if (_functions.TryGetValue(source, out func))
                return func;
            var script = "lambda v:" + source;
            var pyfunc = _engine.Execute(script, _scope);
            func = _engine.Operations.ConvertTo<Func<object, object>>(pyfunc);
            _functions[source] = func;
            return func;
        }

        object IValueConverter.Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (parameter == null)
                throw new NullReferenceException("parameter");
            if (value == DependencyProperty.UnsetValue)
                value = null;

            try
            {
                var func = DefineFunction(parameter.ToString());
                return func(value);
            }
            catch
            {
                return value;
            }
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        object IMultiValueConverter.Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            values = values.Select(v => v == DependencyProperty.UnsetValue ? null : v).ToArray();
            return ((IValueConverter)this).Convert(values, targetType, parameter, culture);
        }

        object[] IMultiValueConverter.ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        static PythonConverter _default = new PythonConverter(new[] { "PresentationFramework", "PresentationCore", "WindowsBase" },
                                                              new[] { "System.Windows" });
        public static PythonConverter Default
        {
            get { return _default; }
        }
    }
}
