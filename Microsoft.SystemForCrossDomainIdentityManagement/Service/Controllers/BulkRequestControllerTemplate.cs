﻿//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace Microsoft.SCIM
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;
    using System.Web.Http;
    using Microsoft.AspNetCore.Mvc;

    [ApiController]
    public abstract class BulkRequestControllerTemplate : ControllerTemplate
    {
        protected BulkRequestControllerTemplate(IProvider provider, IMonitor monitor)
            : base(provider, monitor)
        {
        }

        public async Task<BulkResponse2> Post([FromBody] BulkRequest2 bulkRequest)
        {
            string correlationIdentifier = null;

            try
            {
                HttpRequestMessage request = this.ConvertRequest();
                if (null == bulkRequest)
                {
                    throw new HttpResponseException(HttpStatusCode.BadRequest);
                }

                if (!request.TryGetRequestIdentifier(out correlationIdentifier))
                {
                    throw new HttpResponseException(HttpStatusCode.InternalServerError);
                }

                IProvider provider = this.provider;
                if (null == provider)
                {
                    throw new HttpResponseException(HttpStatusCode.InternalServerError);
                }

                IReadOnlyCollection<IExtension> extensions = provider.ReadExtensions();
                IRequest<BulkRequest2> request2 = new BulkRequest(request, bulkRequest, correlationIdentifier, extensions);
                BulkResponse2 result = await provider.ProcessAsync(request2).ConfigureAwait(false);
                return result;
                
            }
            catch (ArgumentException argumentException)
            {
                if (this.TryGetMonitor(out IMonitor monitor))
                {
                    IExceptionNotification notification =
                        ExceptionNotificationFactory.Instance.CreateNotification(
                            argumentException,
                            correlationIdentifier,
                            ServiceNotificationIdentifiers.BulkRequest2ControllerPostArgumentException);
                    monitor.Report(notification);
                }

                throw new HttpResponseException(HttpStatusCode.BadRequest);
            }
            catch (NotImplementedException notImplementedException)
            {
                if (this.TryGetMonitor(out IMonitor monitor))
                {
                    IExceptionNotification notification =
                        ExceptionNotificationFactory.Instance.CreateNotification(
                            notImplementedException,
                            correlationIdentifier,
                            ServiceNotificationIdentifiers.BulkRequest2ControllerPostNotImplementedException);
                    monitor.Report(notification);
                }
                throw new HttpResponseException(HttpStatusCode.NotImplemented);
            }
            catch (NotSupportedException notSupportedException)
            {
                if (this.TryGetMonitor(out IMonitor monitor))
                {
                    IExceptionNotification notification =
                        ExceptionNotificationFactory.Instance.CreateNotification(
                            notSupportedException,
                            correlationIdentifier,
                            ServiceNotificationIdentifiers.BulkRequest2ControllerPostNotSupportedException);
                    monitor.Report(notification);
                }

                throw new HttpResponseException(HttpStatusCode.NotImplemented);
            }
            catch (Exception exception)
            {
                if (this.TryGetMonitor(out IMonitor monitor))
                {
                    IExceptionNotification notification =
                        ExceptionNotificationFactory.Instance.CreateNotification(
                            exception,
                            correlationIdentifier,
                            ServiceNotificationIdentifiers.BulkRequest2ControllerPostException);
                    monitor.Report(notification);
                }

                throw;
            }
        }
    }
}