﻿using IocPerformance.Classes.Generics;
//

namespace IocPerformance.Classes.Generics
{
    //
    //
    public class GenericExport<T> : IGenericInterface<T>
    {
        public T Value { get; set; }
    }
}
