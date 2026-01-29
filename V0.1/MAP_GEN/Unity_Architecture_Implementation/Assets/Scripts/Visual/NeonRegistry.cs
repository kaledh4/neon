using UnityEngine;
using System.Collections.Generic;

namespace Visual
{
    /// <summary>
    /// Global Registry that controls the lighting state of the entire world.
    /// NeonBinders subscribe to this.
    /// </summary>
    public class NeonRegistry : MonoBehaviour
    {
        public static NeonRegistry Instance;

        // Runtime dictionary of active channels
        private Dictionary<string, NeonChannelController> channels = new Dictionary<string, NeonChannelController>();
        
        // List for inspector editing
        public List<NeonChannelController> definedChannels = new List<NeonChannelController>();

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);

            InitializeChannels();
        }

        private void InitializeChannels()
        {
            foreach (var channel in definedChannels)
            {
                if (channel != null && !channels.ContainsKey(channel.channelName))
                {
                    channels.Add(channel.channelName, channel);
                }
            }
        }

        public NeonChannelController GetChannel(string channelName)
        {
            if (string.IsNullOrEmpty(channelName)) return null;

            if (channels.TryGetValue(channelName, out var controller))
            {
                return controller;
            }
            
            // Fallback: create a default temporary channel if not found to avoid crash
            // Debug.LogWarning($"[NeonRegistry] Channel '{channelName}' not found.");
            return null;
        }

        public void RegisterChannel(NeonChannelController channel)
        {
            if (!channels.ContainsKey(channel.channelName))
            {
                channels.Add(channel.channelName, channel);
            }
        }
    }
}
