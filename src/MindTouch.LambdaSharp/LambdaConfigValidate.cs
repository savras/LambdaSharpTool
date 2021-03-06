﻿/*
 * MindTouch λ#
 * Copyright (C) 2018 MindTouch, Inc.
 * www.mindtouch.com  oss@mindtouch.com
 *
 * For community documentation and downloads visit mindtouch.com;
 * please review the licensing section.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using System;

namespace MindTouch.LambdaSharp {

    public class LambdaConfigValidationException : ALambdaConfigException {

        //--- Constructors ---
        public LambdaConfigValidationException(string message) : base(message) { }
    }

    public static class LambdaConfigValidate {

        //--- Class Methods ---
        public static void IsPositive(int value) {
            if(value <= 0) {
                throw new LambdaConfigValidationException("value is not positive");
            }
        }

        public static void IsNonPositive(int value) {
            if(value > 0) {
                throw new LambdaConfigValidationException("value is not non-positive");
            }
        }

        public static void IsNegative(int value) {
            if(value >= 0) {
                throw new LambdaConfigValidationException("value is not negative");
            }
        }

        public static void IsNonNegative(int value) {
            if(value < 0) {
                throw new LambdaConfigValidationException("value is not non-negative");
            }
        }

        public static Action<int> IsInRange(int low, int high) => value => {
            if((value < low) || (value > high)) {
                throw new LambdaConfigValidationException($"value is in range: [{low}..{high}]");
            }
        };
    }
}