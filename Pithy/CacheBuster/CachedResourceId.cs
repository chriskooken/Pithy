﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pithy.CacheBuster
{
    public static class CachedResourceId
    {
        private static bool configured;
        private static bool autoGenerated;
        public static bool AutoGenerated
        {
            get { return autoGenerated; }
            set
            {
                if (configured)
                    throw new InvalidOperationException("This property has already been configured");
                autoGenerated = value;
                configured = true;
            }
        }

        private static string key;
        public static string Key
        {
            get
            {
                if (!configured)
                    throw new InvalidOperationException("Please set the AutoGenerated property before getting the Key");
                if (autoGenerated)
                    return DateTime.Now.Ticks.ToString();
                if (string.IsNullOrEmpty(key))
                    throw new InvalidOperationException("Key has been set and the AutoGenerated property is set to FALSE");
                return key;
            }
            set
            {
                if (!configured)
                    throw new InvalidOperationException("Please set the AutoGenerated property to FALSE before setting the Key");
                if (autoGenerated)
                    throw new InvalidOperationException("Cannot set Key when the AutoGenerated property is set to TRUE");
                key = value;
            }
        }
    }
}
