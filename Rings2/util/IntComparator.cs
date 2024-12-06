/*
 * Redberry: symbolic tensor computations.
 *
 * Copyright (c) 2010-2015:
 *   Stanislav Poslavsky   <stvlpos@mail.ru>
 *   Bolotin Dmitriy       <bolotin.dmitriy@gmail.com>
 *
 * This file is part of Redberry.
 *
 * Redberry is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * Redberry is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with Redberry. If not, see <http://www.gnu.org/licenses/>.
 */
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using static Cc.Redberry.Rings.Util.RoundingMode;
using static Cc.Redberry.Rings.Util.Associativity;
using static Cc.Redberry.Rings.Util.Operator;
using static Cc.Redberry.Rings.Util.TokenType;
using static Cc.Redberry.Rings.Util.SystemInfo;

namespace Cc.Redberry.Rings.Util
{
    public interface IntComparator
    {
        int Compare(int a, int b);
        IntComparator DEFAULT = (a, b) => Integer.Compare(a, b);
    }
}