using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NotificationHubsSASGenerator
{
    internal class ComboBoxItem<T>
    {
        private string _description;
        private T _value;

        // ====================================================================================================
        public ComboBoxItem(string description, T value)
        {
            this._description = description;
            this._value = value;
        }

        // ====================================================================================================
        public string Description
        {
            get
            {
                return this._description;
            }
        }

        // ====================================================================================================
        public T Value
        {
            get
            {
                return this._value;
            }
        }

        // ====================================================================================================
        public override string ToString()
        {
            return this._description;
        }
    }
}

