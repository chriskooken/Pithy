// Copyright (c) Microsoft Corporation.  All rights reserved.
// This source code is made available under the terms of the Microsoft Public License (MS-PL)

using System;

namespace Pithy
{
    /// <summary>
    /// A compound key is composed of 0-n objects which are hashed together (using exclusive-or) to produce a single hash.
    /// </summary>
    internal class CompoundKey : IEquatable<CompoundKey>
    {
        private object[] values;
        private int hc;

        /// <summary>
        /// Initializes a new instance of the <see cref="CompoundKey"/> class.
        /// </summary>
        /// <param name="values">The values.</param>
        public CompoundKey(params object[] values)
        {
            this.values = values;
            for (int i = 0, n = values.Length; i < n; i++)
            {
                object value = values[i];
                if (value != null)
                {
                    hc ^= (value.GetHashCode() + i);
                }
            }
        }

        /// <summary>
        /// Serves as a hash function for a particular type.
        /// </summary>
        /// <returns>
        /// A hash code for the current <see cref="T:System.Object"/>.
        /// </returns>
        public override int GetHashCode()
        {
            return hc;
        }

        /// <summary>
        /// Determines whether the specified <see cref="T:System.Object"/> is equal to the current <see cref="T:System.Object"/>.
        /// </summary>
        /// <param name="obj">The <see cref="T:System.Object"/> to compare with the current <see cref="T:System.Object"/>.</param>
        /// <returns>
        /// true if the specified <see cref="T:System.Object"/> is equal to the current <see cref="T:System.Object"/>; otherwise, false.
        /// </returns>
        /// <exception cref="T:System.NullReferenceException">
        /// The <paramref name="obj"/> parameter is null.
        /// </exception>
        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>
        /// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
        /// </returns>
        public bool Equals(CompoundKey other)
        {
            if (other == null || other.values.Length != values.Length)
                return false;
            for (int i = 0, n = other.values.Length; i < n; i++)
            {
                if (!object.Equals(this.values[i], other.values[i]))
                    return false;
            }
            return true;
        }
    }

}
