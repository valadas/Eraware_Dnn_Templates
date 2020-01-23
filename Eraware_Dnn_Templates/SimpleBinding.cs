using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Markup;

namespace Eraware_Dnn_Templates
{
    public class SimpleBinding : Binding
    {
        public SimpleBinding(string path) : this()
        {
            Path = new System.Windows.PropertyPath(path);
        }

        public SimpleBinding()
        {
            UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            Mode = BindingMode.TwoWay;
            ValidatesOnExceptions = true;
        }
    }
}
