/*
 * Copyright (c) 1999, 2007, Oracle and/or its affiliates. All rights reserved.
 * ORACLE PROPRIETARY/CONFIDENTIAL. Use is subject to license terms.
 *
 *
 *
 *
 *
 *
 *
 *
 *
 *
 *
 *
 *
 *
 *
 *
 *
 *
 *
 *
 */
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using static Cc.Redberry.Rings.Bigint.RoundingMode;

namespace Cc.Redberry.Rings.Bigint
{
    /// <summary>
    /// A class used to represent multiprecision integers that makes efficient
    /// use of allocated space by allowing a number to occupy only part of
    /// an array so that the arrays do not have to be reallocated as often.
    /// When performing an operation with many iterations the array used to
    /// hold a number is only increased when necessary and does not have to
    /// be the same size as the number it represents. A mutable number allows
    /// calculations to occur on the same number without having to create
    /// a new number for every step of the calculation as occurs with
    /// BigIntegers.
    /// 
    /// Note that SignedMutableBigIntegers only support signed addition and
    /// subtraction. All other operations occur as with MutableBigIntegers.
    /// </summary>
    /// <remarks>
    /// @see    BigInteger
    /// @author Michael McCloskey
    /// @since  1.3
    /// </remarks>
    class SignedMutableBigInteger : MutableBigInteger
    {
        /// <summary>
        /// The sign of this MutableBigInteger.
        /// </summary>
        int sign = 1;
        /// <summary>
        /// The sign of this MutableBigInteger.
        /// </summary>
        // Constructors
        /// <summary>
        /// The default constructor. An empty MutableBigInteger is created with
        /// a one word capacity.
        /// </summary>
        SignedMutableBigInteger() : base()
        {
        }

        /// <summary>
        /// The sign of this MutableBigInteger.
        /// </summary>
        // Constructors
        /// <summary>
        /// The default constructor. An empty MutableBigInteger is created with
        /// a one word capacity.
        /// </summary>
        /// <summary>
        /// Construct a new MutableBigInteger with a magnitude specified by
        /// the int val.
        /// </summary>
        SignedMutableBigInteger(int val) : base(val)
        {
        }

        /// <summary>
        /// The sign of this MutableBigInteger.
        /// </summary>
        // Constructors
        /// <summary>
        /// The default constructor. An empty MutableBigInteger is created with
        /// a one word capacity.
        /// </summary>
        /// <summary>
        /// Construct a new MutableBigInteger with a magnitude specified by
        /// the int val.
        /// </summary>
        /// <summary>
        /// Construct a new MutableBigInteger with a magnitude equal to the
        /// specified MutableBigInteger.
        /// </summary>
        SignedMutableBigInteger(MutableBigInteger val) : base(val)
        {
        }

        /// <summary>
        /// The sign of this MutableBigInteger.
        /// </summary>
        // Constructors
        /// <summary>
        /// The default constructor. An empty MutableBigInteger is created with
        /// a one word capacity.
        /// </summary>
        /// <summary>
        /// Construct a new MutableBigInteger with a magnitude specified by
        /// the int val.
        /// </summary>
        /// <summary>
        /// Construct a new MutableBigInteger with a magnitude equal to the
        /// specified MutableBigInteger.
        /// </summary>
        // Arithmetic Operations
        /// <summary>
        /// Signed addition built upon unsigned add and subtract.
        /// </summary>
        virtual void SignedAdd(SignedMutableBigInteger addend)
        {
            if (sign == addend.sign)
                Add(addend);
            else
                sign = sign * Subtract(addend);
        }

        /// <summary>
        /// The sign of this MutableBigInteger.
        /// </summary>
        // Constructors
        /// <summary>
        /// The default constructor. An empty MutableBigInteger is created with
        /// a one word capacity.
        /// </summary>
        /// <summary>
        /// Construct a new MutableBigInteger with a magnitude specified by
        /// the int val.
        /// </summary>
        /// <summary>
        /// Construct a new MutableBigInteger with a magnitude equal to the
        /// specified MutableBigInteger.
        /// </summary>
        // Arithmetic Operations
        /// <summary>
        /// Signed addition built upon unsigned add and subtract.
        /// </summary>
        /// <summary>
        /// Signed addition built upon unsigned add and subtract.
        /// </summary>
        virtual void SignedAdd(MutableBigInteger addend)
        {
            if (sign == 1)
                Add(addend);
            else
                sign = sign * Subtract(addend);
        }

        /// <summary>
        /// The sign of this MutableBigInteger.
        /// </summary>
        // Constructors
        /// <summary>
        /// The default constructor. An empty MutableBigInteger is created with
        /// a one word capacity.
        /// </summary>
        /// <summary>
        /// Construct a new MutableBigInteger with a magnitude specified by
        /// the int val.
        /// </summary>
        /// <summary>
        /// Construct a new MutableBigInteger with a magnitude equal to the
        /// specified MutableBigInteger.
        /// </summary>
        // Arithmetic Operations
        /// <summary>
        /// Signed addition built upon unsigned add and subtract.
        /// </summary>
        /// <summary>
        /// Signed addition built upon unsigned add and subtract.
        /// </summary>
        /// <summary>
        /// Signed subtraction built upon unsigned add and subtract.
        /// </summary>
        virtual void SignedSubtract(SignedMutableBigInteger addend)
        {
            if (sign == addend.sign)
                sign = sign * Subtract(addend);
            else
                Add(addend);
        }

        /// <summary>
        /// The sign of this MutableBigInteger.
        /// </summary>
        // Constructors
        /// <summary>
        /// The default constructor. An empty MutableBigInteger is created with
        /// a one word capacity.
        /// </summary>
        /// <summary>
        /// Construct a new MutableBigInteger with a magnitude specified by
        /// the int val.
        /// </summary>
        /// <summary>
        /// Construct a new MutableBigInteger with a magnitude equal to the
        /// specified MutableBigInteger.
        /// </summary>
        // Arithmetic Operations
        /// <summary>
        /// Signed addition built upon unsigned add and subtract.
        /// </summary>
        /// <summary>
        /// Signed addition built upon unsigned add and subtract.
        /// </summary>
        /// <summary>
        /// Signed subtraction built upon unsigned add and subtract.
        /// </summary>
        /// <summary>
        /// Signed subtraction built upon unsigned add and subtract.
        /// </summary>
        virtual void SignedSubtract(MutableBigInteger addend)
        {
            if (sign == 1)
                sign = sign * Subtract(addend);
            else
                Add(addend);
            if (intLen == 0)
                sign = 1;
        }

        /// <summary>
        /// The sign of this MutableBigInteger.
        /// </summary>
        // Constructors
        /// <summary>
        /// The default constructor. An empty MutableBigInteger is created with
        /// a one word capacity.
        /// </summary>
        /// <summary>
        /// Construct a new MutableBigInteger with a magnitude specified by
        /// the int val.
        /// </summary>
        /// <summary>
        /// Construct a new MutableBigInteger with a magnitude equal to the
        /// specified MutableBigInteger.
        /// </summary>
        // Arithmetic Operations
        /// <summary>
        /// Signed addition built upon unsigned add and subtract.
        /// </summary>
        /// <summary>
        /// Signed addition built upon unsigned add and subtract.
        /// </summary>
        /// <summary>
        /// Signed subtraction built upon unsigned add and subtract.
        /// </summary>
        /// <summary>
        /// Signed subtraction built upon unsigned add and subtract.
        /// </summary>
        /// <summary>
        /// Print out the first intLen ints of this MutableBigInteger's value
        /// array starting at offset.
        /// </summary>
        public virtual string ToString()
        {
            return this.ToBigInteger(sign).ToString();
        }
    }
}