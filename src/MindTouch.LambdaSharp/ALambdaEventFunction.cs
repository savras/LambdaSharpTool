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
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Amazon.Lambda.Core;
using Amazon.Lambda.SNSEvents;
using Newtonsoft.Json;

namespace MindTouch.LambdaSharp {

    public abstract class ALambdaEventFunction<TMessage> : ALambdaFunction {

        //--- Constructors ---
        protected ALambdaEventFunction() : this(LambdaFunctionConfiguration.Instance) { }

        protected ALambdaEventFunction(LambdaFunctionConfiguration configuration) : base(configuration) { }

        //--- Abstract Methods ---
        public abstract Task ProcessMessageAsync(TMessage message, ILambdaContext context);

        //--- Methods ---
        public virtual TMessage Deserialize(string body) 
            => JsonConvert.DeserializeObject<TMessage>(body);

        public override async Task<object> ProcessMessageStreamAsync(Stream stream, ILambdaContext context) {

            // read stream into memory
            LogInfo("reading message stream");
            string snsEventBody;
            try {
                using(var reader = new StreamReader(stream)) {
                    snsEventBody = reader.ReadToEnd();
                }
            } catch(Exception e) {
                LogError(e, "failed during message stream reading");
                throw;
            }

            // sns event deserialization
            LogInfo("deserializing SNS event");
            SNSEvent snsEvent;
            try {
                snsEvent = JsonConvert.DeserializeObject<SNSEvent>(snsEventBody);
            } catch(Exception e) {
                LogError(e, "failed during event deserialization");
                await RecordFailedMessageAsync(LambdaLogLevel.ERROR, snsEventBody, e);
                return $"ERROR: {e.Message}";
            }

            // message deserialization
            LogInfo("deserializing message");
            string messageBody;
            try {
                messageBody = snsEvent.Records.First().Sns.Message;
            } catch(Exception e) {
                LogError(e, "failed accessing message body");
                await RecordFailedMessageAsync(LambdaLogLevel.ERROR, snsEventBody, e);
                return $"ERROR: {e.Message}";
            }
            TMessage message;
            try {
                message = Deserialize(messageBody);
            } catch(Exception e) {
                LogError(e, "failed during message deserialization");
                await RecordFailedMessageAsync(LambdaLogLevel.ERROR, snsEventBody, e);
                return $"ERROR: {e.Message}";
            }

            // process message
            LogInfo("processing message");
            try {
                await ProcessMessageAsync(message, context);
                return "Ok";
            } catch(Exception e) when(!(e is ALambdaRetriableException)) {

                // TODO (2018-06-14, bjorg): revisit how exceptions are surfaced
                LogError(e, "failed during message processing");
                await RecordFailedMessageAsync(LambdaLogLevel.ERROR, snsEventBody, e);
                return $"ERROR: {e.Message}";
            }
        }
    }
}
