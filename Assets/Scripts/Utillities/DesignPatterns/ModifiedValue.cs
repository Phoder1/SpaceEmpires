using Phoder1.Async;
using Phoder1.Core.Stats;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
namespace Phoder1.Core
{
    public enum EventLevel
    {
        Normal = 0,
        NoVisuals = 1,
        NoUI = 2,
        Silent = 3
    }
    [InlineProperty]
    [Serializable]
    public class ModifiedValue<T> : IDisposable, IResetable, IValue<T>, IModifiedValue<T>
    {
        #region Serialized
        public event Action<T> OnAfterValueChanged;
        public event Action<T, T> OnValueChanged;
        public event Action<T> OnBeforeValueChanged;
        [SerializeField]
        protected T _startValue;

        [SerializeField]
        protected T _currentBaseValue;
        #endregion
        #region Class data

        private List<IModifier<T>> _modifiers = new List<IModifier<T>>();
        [ShowInInspector]
        private T _value = default;

        /// <summary>
        /// The number of modifiers with the  CacheEveryUpdate flag active.
        /// </summary>
        private int _runtimeUpdateModifiersCount = 0;

        private bool _isAlive = true;
        private bool _cacheNeeded = false;
        #endregion
        #region Properties
        public IReadOnlyList<IModifier<T>> Modifiers => _modifiers;
        public virtual T BaseValue
        {
            get => _currentBaseValue;
            set
            {
                if (Equals(_currentBaseValue, value))
                    return;

                _currentBaseValue = value;


                ForceCacheValue();
            }
        }

        public T Value
        {
            get => _value;
            protected set => SetValue(value);
        }

        public void SetValue(T value)
        {
            if (_value.IsEquals(value))
                return;

            OnBeforeValueChanged?.Invoke(_value);

            var oldValue = _value;
            _value = value;

            OnAfterValueChanged?.Invoke(_value);

            OnValueChanged?.Invoke(oldValue, _value);
        }
        public T StartValue { get => _startValue; set => _startValue = value; }
        #endregion
        #region Costructors
        public ModifiedValue(T startValue, List<IModifier<T>> modifiers = null)
        {
            _startValue = startValue;
            _modifiers = modifiers ?? new List<IModifier<T>>();
            _currentBaseValue = _startValue;

            ForceCacheValue();
        }

        ~ModifiedValue()
        {
            _isAlive = false;
        }

        public async void UpdateAsync()
        {
            do
            {
                await Task.Yield();

                if (_cacheNeeded || _runtimeUpdateModifiersCount > 0)
                    ForceCacheValue();
            } while (_isAlive && !AsyncManager.GlobalToken.IsCancellationRequested) ;


        }
        #endregion
        #region Public methods
        public virtual T GetModifiedValue()
        {
            if (_modifiers == null || _modifiers.Count == 0)
                return BaseValue;

            Sort();

            T modifiedValue = BaseValue;

            for (int i = 0; i < _modifiers.Count; i++)
            {

                if (_modifiers[i] == null || _modifiers[i].CheckModifier() == false)
                {
                    _modifiers.RemoveAt(i);
                    i--;
                    continue;
                }
                modifiedValue = ApplyModifier(modifiedValue, _modifiers[i]);
            }


            return modifiedValue;
        }

        protected virtual T ApplyModifier(T modifiedValue, IModifier<T> modifier)
             => modifier.ApplyModifier(modifiedValue);
        public IDisposable AddModifier(IFactory<IModifier<T>> modifier)
        {
            var modInstance = modifier.Create();
            AddModifier(modInstance);

            return new Disposeable(Remove);

            void Remove() => RemoveModifier(modInstance);
        } 
        private bool AddModifier(IModifier<T> modifier)
        {
            if (_modifiers == null)
                _modifiers = new List<IModifier<T>>();

            if (_modifiers.Contains(modifier))
                return false;

            _modifiers.Add(modifier);

            modifier.OnAdd((IReadOnlyList<IModifier<T>>)_modifiers);
            Sort();
            if (modifier.CacheEveryUpdate)
                _runtimeUpdateModifiersCount++;

            modifier.UpdateCache += CacheRequested;


            ForceCacheValue();
            return true;
        }

        private bool RemoveModifier(IModifier<T> modifier)
        {
            if (_modifiers == null)
                _modifiers = new List<IModifier<T>>();

            bool success = _modifiers?.Remove(modifier) ?? false;
            if (success)
            {
                modifier.OnRemove((IReadOnlyList<IModifier<T>>)_modifiers);

                if (modifier.CacheEveryUpdate)
                    _runtimeUpdateModifiersCount--;
            }

            modifier.UpdateCache -= CacheRequested;

            ForceCacheValue();

            return success;
        }
        public void DoReset()
        {
            if (_modifiers == null)
                _modifiers = new List<IModifier<T>>();
            else
                _modifiers.Clear();

            _currentBaseValue = _startValue;

            OnReset();

            ForceCacheValue();
        }
        protected virtual void OnReset() { }
        #endregion
        #region Private methods
        protected void CacheRequested() => _cacheNeeded = true;
        protected virtual void Sort() => _modifiers.Sort((x, y) => x.Priority.CompareTo(y?.Priority ?? int.MinValue));
        protected virtual void ForceCacheValue()
        {
            T previousValue = GetModifiedValue();
            ValueChanged(previousValue, Value);
            Value = previousValue; 

            _cacheNeeded = false;
        }
        protected virtual void ValueChanged(T previousValue, T nextValue)
            => OnValueChanged?.Invoke(previousValue, nextValue);
        protected void InvokeOnValueModified(T value) 
            => OnAfterValueChanged?.Invoke(value);
        protected void InvokeOnRecievingModifiedValue(T value) 
            => OnBeforeValueChanged?.Invoke(value);

        public void Dispose()
        {
            _isAlive = false;
        }
        #endregion
    }
}
