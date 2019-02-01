﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using log4net.Core;
using log4net.Util;

namespace Log4Mongo
{
	public class BackwardCompatibility
	{
		public static MongoDatabase GetDatabase(MongoDBAppender appender)
		{
			var port = appender.Port > 0 ? appender.Port : 27017;
			var mongoConnectionString = new StringBuilder(string.Format("Server={0}:{1}", appender.Host ?? "localhost", port));
			if(!string.IsNullOrEmpty(appender.UserName) && !string.IsNullOrEmpty(appender.Password))
			{
				// use MongoDB authentication
				mongoConnectionString.AppendFormat(";Username={0};Password={1}", appender.UserName, appender.Password);
			}

			MongoServer connection = MongoServer.Create(mongoConnectionString.ToString());
			connection.Connect();
			return connection.GetDatabase(appender.DatabaseName ?? "log4net_mongodb");
		}

		public static BsonDocument BuildBsonDocument(LoggingEvent loggingEvent)
		{
            Debug.WriteLine("Log4Mongo writing @level {0}" + loggingEvent.Level);

            if(loggingEvent==null)
                return null;

            //Sudip*********
		    var docToReturn = loggingEvent.MessageObject.ToBsonDocument();
            if (loggingEvent.ExceptionObject != null)
            {
                docToReturn.Add("Exception", BuildExceptionBsonDocument(loggingEvent.ExceptionObject));
            }
            return docToReturn;

            //Sudip*********

            /*
			var toReturn = new BsonDocument {
				{"timestamp", loggingEvent.TimeStamp}, 
				{"level", loggingEvent.Level.ToString()}, 
				{"thread", loggingEvent.ThreadName}, 
				{"userName", loggingEvent.UserName}, 
				//{"message", loggingEvent.RenderedMessage}, 
                {"message", loggingEvent.MessageObject.ToBsonDocument()},
				{"loggerName", loggingEvent.LoggerName}, 
				{"domain", loggingEvent.Domain}, 
				{"machineName", Environment.MachineName}
			};

			// location information, if available
			if(loggingEvent.LocationInformation != null)
			{
				toReturn.Add("fileName", loggingEvent.LocationInformation.FileName);
				toReturn.Add("method", loggingEvent.LocationInformation.MethodName);
				toReturn.Add("lineNumber", loggingEvent.LocationInformation.LineNumber);
				toReturn.Add("className", loggingEvent.LocationInformation.ClassName);
			}

			// exception information
			if(loggingEvent.ExceptionObject != null)
			{
				toReturn.Add("exception", BuildExceptionBsonDocument(loggingEvent.ExceptionObject));
			}

			// properties
			PropertiesDictionary compositeProperties = loggingEvent.GetProperties();
			if(compositeProperties != null && compositeProperties.Count > 0)
			{
				var properties = new BsonDocument();
				foreach(DictionaryEntry entry in compositeProperties)
				{
					properties.Add(entry.Key.ToString(), entry.Value.ToString());
				}

				toReturn.Add("properties", properties);
			}

			return toReturn;
             */
		}

		private static BsonDocument BuildExceptionBsonDocument(Exception ex)
		{
			var toReturn = new BsonDocument {
				{"message", ex.Message}, 
				{"source", ex.Source}, 
				{"stackTrace", ex.StackTrace}
			};

			if(ex.InnerException != null)
			{
				toReturn.Add("InnerException", BuildExceptionBsonDocument(ex.InnerException));
			}

			return toReturn;
		}
	}
}