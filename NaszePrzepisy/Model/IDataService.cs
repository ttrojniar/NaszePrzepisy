using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NaszePrzepisy.Model
{
    public interface IDataService
    {
        void GetData(Action<DataItem, Exception> callback);
    }
}
