using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
 
// <summary>
// Remembers that an action was pressed for a short window, together with a
// UnityAction to run once something says it's allowed. Not tied to the
// state machine, or even to input specifically - buffer any key with any
// callback and consume it from wherever the relevant condition lives.
//
// Generic by design: the same buffered signal can be consumed by different
// systems with different rules. e.g. buffer "attack" with a default action
// of "fire the attack trigger", but let a combo-window inside the attack
// state itself consume the same entry with a different action ("chain into
// another swing") - see TryConsume's overrideAction parameter.
// </summary>

namespace Zlipacket.Core.Input
{
    public class InputBuffer
    {
        private struct Entry
        {
            public UnityAction action;
            public float expireTime;
        }
     
        private readonly Dictionary<string, Entry> pending = new Dictionary<string, Entry>();
     
        /// Call the moment the raw input happens, e.g. on Input.GetButtonDown("Fire1").
        /// `action` is the default thing to do once it's allowed to fire - pass
        /// null if every consumer will supply its own action via TryConsume.
        public void Buffer(string key, float window, UnityAction action = null)
        {
            pending[key] = new Entry { action = action, expireTime = Time.time + window };
        }
     
        /// True if this key was buffered and the window hasn't expired.
        public bool IsPending(string key) =>
            pending.TryGetValue(key, out var e) && e.expireTime >= Time.time;
     
        /// Call whenever you have a chance to spend a buffered input.
        /// - Returns false immediately if nothing is buffered under this key.
        /// - Silently drops (and returns false for) an expired entry.
        /// - Otherwise, if canFire is null or returns true: removes the entry,
        ///   invokes overrideAction if given, otherwise the action captured at
        ///   Buffer() time, and returns true.
        public bool TryConsume(string key, Func<bool> canFire = null, UnityAction overrideAction = null)
        {
            if (!pending.TryGetValue(key, out var e)) return false;
     
            if (e.expireTime < Time.time)
            {
                pending.Remove(key); // stale - don't let it fire late
                return false;
            }
     
            if (canFire != null && !canFire()) return false;
     
            pending.Remove(key);
            (overrideAction ?? e.action)?.Invoke();
            return true;
        }
     
        /// Drops a buffered entry without invoking anything.
        public void Consume(string key) => pending.Remove(key);
     
        /// Optional housekeeping - TryConsume/IsPending already treat expired
        /// entries as absent, this just keeps the dictionary from growing stale keys.
        public void Prune()
        {
            if (pending.Count == 0) return;
            List<string> expired = null;
            foreach (var kv in pending)
                if (kv.Value.expireTime < Time.time)
                    (expired ??= new List<string>()).Add(kv.Key);
            if (expired != null)
                foreach (var key in expired) pending.Remove(key);
        }
    }
}