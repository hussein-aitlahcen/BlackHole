using System;
using System.Collections.Generic;

namespace BlackHole.Common
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TEvent"></typeparam>
    /// <typeparam name="TSource"></typeparam>
    public interface IEventListener<TEvent, TSource>
        where TEvent : Event<TSource>
    {
        void OnEvent(TEvent ev);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class Event<T>
    {
        public int EventType { get; }
        public T Source { get; }
        public object Data { get; }

        protected Event(int eventType, T source, object data)
        {
            EventType = eventType;
            Source = source;
            Data = data;
        }
    }
        
    /// <summary>
    /// 
    /// </summary>
    public sealed class EventBus<TEvent, TSource>
        where TEvent : Event<TSource>
    {
        /// <summary>
        /// 
        /// </summary>
        private class Subscriber
        {
            public Predicate<TEvent> Guard
            {
                get;
            }
            public IEventListener<TEvent, TSource> Listener
            {
                get;
            }

            public Subscriber(Predicate<TEvent> guard, IEventListener<TEvent, TSource> listener)
            {
                Guard = guard;
                Listener = listener;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private readonly List<Subscriber> m_subscriber;

        /// <summary>
        /// 
        /// </summary>
        public EventBus()
        {
            m_subscriber = new List<Subscriber>();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="listener"></param>
        public void Subscribe(IEventListener<TEvent, TSource> listener) => Subscribe(e => true, listener);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="guard"></param>
        /// <param name="listener"></param>
        public void Subscribe(Predicate<TEvent> guard, IEventListener<TEvent, TSource> listener) 
            => m_subscriber.Add(new Subscriber(guard, listener));
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="listener"></param>
        public void Unsubscribe(IEventListener<TEvent, TSource> listener) 
            => m_subscriber.RemoveAll(s => s.Listener == listener);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ev"></param>
        public void PostEvent(TEvent ev) => 
            m_subscriber.ForEach(s => 
            {
                if (s.Guard(ev))
                {
                    try
                    {
                        s.Listener.OnEvent(ev);
                    }
                    catch (Exception e)
                    {
                    }
                }
            });        
    }
}
