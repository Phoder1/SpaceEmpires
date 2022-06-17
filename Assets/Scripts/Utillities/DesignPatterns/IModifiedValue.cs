using Phoder1.Core.Effects;
using System;
using System.Collections.Generic;

namespace Phoder1.Core.Value
{
    public interface IValue<T> : IResetable
    {
        T Value { get; }
        T BaseValue { get; set; }
        T StartValue { get; set; }
    }
}