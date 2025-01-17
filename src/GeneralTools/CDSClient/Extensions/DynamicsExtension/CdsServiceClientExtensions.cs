﻿using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Microsoft.PowerPlatform.Cds.Client.Dynamics
{
	/// <summary>
	/// Dynamics Application Extentions for CDSServiceClient
	/// </summary>
    public static class CdsServiceClientExtensions
    {
		/// <summary>
		/// Closes a quote as won or lost,
		/// Revise is not supported via this method
		/// </summary>
		/// <param name="quoteId">ID of the quote to close</param>
		/// <param name="fieldList">List of fields that need to be updated</param>
		/// <param name="quoteStatusCode">Status id of the quote,  must be greater then 3 but not 7</param>
		/// <param name="batchId">Optional: if set to a valid GUID, generated by the Create Batch Request Method, will assigned the request to the batch for later execution, on fail, runs the request immediately </param>
		/// <param name="cdsServiceClient">Connected CDS Service Client</param>
		/// <param name="bypassPluginExecution">Adds the bypass plugin behavior to this request. Note: this will only apply if the caller has the prvBypassPlugins permission to bypass plugins.  If its attempted without the permission the request will fault.</param>
		/// <returns></returns>
		public static Guid CloseQuote(this CdsServiceClient cdsServiceClient , Guid quoteId, Dictionary<string, CdsDataTypeWrapper> fieldList, int quoteStatusCode = 3, Guid batchId = default(Guid), bool bypassPluginExecution = false)
		{
			cdsServiceClient.logEntry.ResetLastError();  // Reset Last Error 
			if (cdsServiceClient._CdsService == null)
			{
				cdsServiceClient.logEntry.Log("Crm Service not initialized", TraceEventType.Error);
				return Guid.Empty;
			}

			if (quoteId == Guid.Empty)
				return Guid.Empty;

			if (quoteStatusCode < 3)
				return Guid.Empty;

			Guid actId = Guid.Empty;
			Entity uEnt = new Entity("quoteclose");
			AttributeCollection PropertyList = new AttributeCollection();

			#region MapCode
			if (fieldList != null)
				foreach (KeyValuePair<string, CdsDataTypeWrapper> field in fieldList)
				{
					cdsServiceClient.AddValueToPropertyList(field, PropertyList);
				}

			// Add the key... 
			// check to see if the key is in the import set already 
			if (fieldList != null && !fieldList.ContainsKey("quoteid"))
				PropertyList.Add(new KeyValuePair<string, object>("quoteid", quoteId));

			if (fieldList != null && fieldList.ContainsKey("activityid"))
				actId = (Guid)fieldList["activityid"].Value;
			else
			{
				actId = Guid.NewGuid();
				uEnt.Id = actId;
			}
			#endregion
			uEnt.Attributes.AddRange(PropertyList.ToArray());

			// 2 types of close supported... Won or Lost. 
			if (quoteStatusCode == 4)
			{
				WinQuoteRequest req = new WinQuoteRequest();
				req.QuoteClose = uEnt;
				req.Status = new OptionSetValue(quoteStatusCode);


				if (cdsServiceClient.AddRequestToBatch(batchId, req, "Calling Close Quote as Won", "Request to Close Quote as Won Queued", bypassPluginExecution))
					return Guid.Empty;

				WinQuoteResponse resp = (WinQuoteResponse)cdsServiceClient.CdsCommand_Execute(req, "Closing a Quote in CRM as Won", bypassPluginExecution);
				if (resp != null)
					return actId;
				else
					return Guid.Empty;
			}
			else
			{
				CloseQuoteRequest req = new CloseQuoteRequest();
				req.QuoteClose = uEnt;
				req.Status = new OptionSetValue(quoteStatusCode);

				if (cdsServiceClient.AddRequestToBatch(batchId, req, "Calling Close Quote as Lost", "Request to Close Quote as Lost Queued", bypassPluginExecution))
					return Guid.Empty;

				CloseQuoteResponse resp = (CloseQuoteResponse)cdsServiceClient.CdsCommand_Execute(req, "Closing a Quote in CRM as Lost", bypassPluginExecution);
				if (resp != null)
					return actId;
				else
					return Guid.Empty;
			}
		}


		/// <summary>
		/// This will close an opportunity as either Won or lost in CRM
		/// </summary>
		/// <param name="opportunityId">ID of the opportunity to close</param>
		/// <param name="fieldList">List of fields for the Opportunity Close Entity</param>
		/// <param name="opportunityStatusCode">Status code of Opportunity, Should be either 1 or 2,  defaults to 1 ( won )</param>
		/// <param name="batchId">Optional: if set to a valid GUID, generated by the Create Batch Request Method, will assigned the request to the batch for later execution, on fail, runs the request immediately </param>
		/// <param name="cdsServiceClient">Connected CDS Service Client</param>
		/// <param name="bypassPluginExecution">Adds the bypass plugin behavior to this request. Note: this will only apply if the caller has the prvBypassPlugins permission to bypass plugins.  If its attempted without the permission the request will fault.</param>
		/// <returns></returns>
		public static Guid CloseOpportunity(this CdsServiceClient cdsServiceClient, Guid opportunityId, Dictionary<string, CdsDataTypeWrapper> fieldList, int opportunityStatusCode = 3, Guid batchId = default(Guid), bool bypassPluginExecution = false)
		{
			cdsServiceClient.logEntry.ResetLastError();  // Reset Last Error 
			if (cdsServiceClient._CdsService == null)
			{
				cdsServiceClient.logEntry.Log("Crm Service not initialized", TraceEventType.Error);
				return Guid.Empty;
			}

			if (opportunityId == Guid.Empty)
				return Guid.Empty;

			if (opportunityStatusCode < 3)
				return Guid.Empty;

			Guid actId = Guid.Empty;
			Entity uEnt = new Entity("opportunityclose");
			AttributeCollection PropertyList = new AttributeCollection();

			#region MapCode
			if (fieldList != null)
				foreach (KeyValuePair<string, CdsDataTypeWrapper> field in fieldList)
				{
					cdsServiceClient.AddValueToPropertyList(field, PropertyList);
				}

			// Add the key... 
			// check to see if the key is in the import set allready 
			if (fieldList != null && !fieldList.ContainsKey("opportunityid"))
				PropertyList.Add(new KeyValuePair<string, object>("opportunityid", opportunityId));

			if (fieldList != null && fieldList.ContainsKey("activityid"))
				actId = (Guid)fieldList["activityid"].Value;
			else
			{
				actId = Guid.NewGuid();
				uEnt.Id = actId;
			}
			#endregion
			uEnt.Attributes.AddRange(PropertyList.ToArray());

			// 2 types of close supported... Won or Lost. 
			if (opportunityStatusCode == 3)
			{
				WinOpportunityRequest req = new WinOpportunityRequest();
				req.OpportunityClose = uEnt;
				req.Status = new OptionSetValue(opportunityStatusCode);

				if (cdsServiceClient.AddRequestToBatch(batchId, req, "Calling Close Opportunity as Won", "Request to Close Opportunity as Won Queued", bypassPluginExecution))
					return Guid.Empty;

				WinOpportunityResponse resp = (WinOpportunityResponse)cdsServiceClient.CdsCommand_Execute(req, "Closing a Opportunity in CRM as Won", bypassPluginExecution);
				if (resp != null)
					return actId;
				else
					return Guid.Empty;
			}
			else
			{
				LoseOpportunityRequest req = new LoseOpportunityRequest();
				req.OpportunityClose = uEnt;
				req.Status = new OptionSetValue(opportunityStatusCode);

				if (cdsServiceClient.AddRequestToBatch(batchId, req, "Calling Close Opportunity as Lost", "Request to Close Opportunity as Lost Queued",bypassPluginExecution))
					return Guid.Empty;

				LoseOpportunityResponse resp = (LoseOpportunityResponse)cdsServiceClient.CdsCommand_Execute(req, "Closing a Opportunity in CRM as Lost",bypassPluginExecution);
				if (resp != null)
					return actId;
				else
					return Guid.Empty;
			}
		}

		/// <summary>
		/// Closes an Incident request in CRM,
		/// this special handling is necessary to support CRM Built In Object.
		/// </summary>
		/// <param name="incidentId">ID of the CRM Incident to close</param>
		/// <param name="fieldList">List of data items to add to the request, By default, subject is required.</param>
		/// <param name="incidentStatusCode">Status code to close the incident with, defaults to resolved</param>
		/// <param name="batchId">Optional: if set to a valid GUID, generated by the Create Batch Request Method, will assigned the request to the batch for later execution, on fail, runs the request immediately </param>
		/// <param name="cdsServiceClient">Connected CDS Service Client</param>
		/// <param name="bypassPluginExecution">Adds the bypass plugin behavior to this request. Note: this will only apply if the caller has the prvBypassPlugins permission to bypass plugins.  If its attempted without the permission the request will fault.</param>
		/// <returns>Guid of the Activity.</returns>
		public static Guid CloseIncident(this CdsServiceClient cdsServiceClient, Guid incidentId, Dictionary<string, CdsDataTypeWrapper> fieldList, int incidentStatusCode = 5, Guid batchId = default(Guid), bool bypassPluginExecution = false)
		{
			cdsServiceClient.logEntry.ResetLastError();  // Reset Last Error 
			if (cdsServiceClient._CdsService == null)
			{
				cdsServiceClient.logEntry.Log("Crm Service not initialized", TraceEventType.Error);
				return Guid.Empty;
			}

			if (incidentId == Guid.Empty)
				return Guid.Empty;

			Guid actId = Guid.Empty;
			Entity uEnt = new Entity("incidentresolution");
			AttributeCollection PropertyList = new AttributeCollection();

			#region MapCode
			if (fieldList != null)
				foreach (KeyValuePair<string, CdsDataTypeWrapper> field in fieldList)
				{
					cdsServiceClient.AddValueToPropertyList(field, PropertyList);
				}

			// Add the key... 
			// check to see if the key is in the import set already 
			if (fieldList != null && !fieldList.ContainsKey("incidentid"))
				PropertyList.Add(new KeyValuePair<string, object>("incidentid", new EntityReference("incident", incidentId)));

			if (fieldList != null && fieldList.ContainsKey("activityid"))
				actId = (Guid)fieldList["activityid"].Value;
			else
			{
				actId = Guid.NewGuid();
				uEnt.Id = actId;
			}
			#endregion
			uEnt.Attributes.AddRange(PropertyList.ToArray());


			CloseIncidentRequest req4 = new CloseIncidentRequest();
			req4.IncidentResolution = uEnt;
			req4.Status = new OptionSetValue(incidentStatusCode);

			if (cdsServiceClient.AddRequestToBatch(batchId, req4, "Calling Close Incident", "Request to Close Incident Queued", bypassPluginExecution))
				return Guid.Empty;


			CloseIncidentResponse resp4 = (CloseIncidentResponse)cdsServiceClient.CdsCommand_Execute(req4, "Closing a incidentId in CRM",bypassPluginExecution);
			if (resp4 != null)
				return actId;
			else
				return Guid.Empty;


		}

		/// <summary>
		/// Cancel Sales order
		/// </summary>
		/// <param name="salesOrderId">Sales order id to close</param>
		/// <param name="fieldList">List of fields to add</param>
		/// <param name="orderStatusCode">Status code of the order</param>
		/// <param name="batchId">Optional: if set to a valid GUID, generated by the Create Batch Request Method, will assigned the request to the batch for later execution, on fail, runs the request immediately </param>
		/// <param name="cdsServiceClient">Connected CDS Service Client</param>
		/// <param name="bypassPluginExecution">Adds the bypass plugin behavior to this request. Note: this will only apply if the caller has the prvBypassPlugins permission to bypass plugins.  If its attempted without the permission the request will fault.</param>
		/// <returns></returns>
		public static Guid CancelSalesOrder(this CdsServiceClient cdsServiceClient, Guid salesOrderId, Dictionary<string, CdsDataTypeWrapper> fieldList, int orderStatusCode = 4, Guid batchId = default(Guid), bool bypassPluginExecution = false)
		{
			cdsServiceClient.logEntry.ResetLastError();  // Reset Last Error 
			if (cdsServiceClient._CdsService == null)
			{
				cdsServiceClient.logEntry.Log("Crm Service not initialized", TraceEventType.Error);
				return Guid.Empty;
			}

			if (salesOrderId == Guid.Empty)
				return Guid.Empty;

			if (orderStatusCode < 4)
				return Guid.Empty;

			Guid actId = Guid.Empty;
			Entity uEnt = new Entity("orderclose");
			AttributeCollection PropertyList = new AttributeCollection();

			#region MapCode
			if (fieldList != null)
				foreach (KeyValuePair<string, CdsDataTypeWrapper> field in fieldList)
				{
					cdsServiceClient.AddValueToPropertyList(field, PropertyList);
				}

			// Add the key... 
			// check to see if the key is in the import set allready 
			if (fieldList != null && !fieldList.ContainsKey("salesorderid"))
				PropertyList.Add(new KeyValuePair<string, object>("salesorderid", salesOrderId));

			if (fieldList != null && fieldList.ContainsKey("activityid"))
				actId = (Guid)fieldList["activityid"].Value;
			else
			{
				actId = Guid.NewGuid();
				uEnt.Id = actId;
			}
			#endregion
			uEnt.Attributes.AddRange(PropertyList.ToArray());

			CancelSalesOrderRequest req = new CancelSalesOrderRequest();
			req.OrderClose = uEnt;
			req.Status = new OptionSetValue(orderStatusCode);

			if (cdsServiceClient.AddRequestToBatch(batchId, req, "Calling Close Sales Order", "Request to Close Sales Order Queued",bypassPluginExecution))
				return Guid.Empty;


			CancelSalesOrderResponse resp = (CancelSalesOrderResponse)cdsServiceClient.CdsCommand_Execute(req, "Closing a Sales Order in CRM as Closed",bypassPluginExecution);
			if (resp != null)
				return actId;
			else
				return Guid.Empty;

		}


		/// <summary>
		/// Closes a Trouble ticket by ID
		/// </summary>
		/// <param name="ticketId">ID of the Ticket to close</param>
		/// <param name="subject">Title of the close ticket record</param>
		/// <param name="description">Description of the closed ticket</param>
		/// <param name="batchId">Optional: if set to a valid GUID, generated by the Create Batch Request Method, will assigned the request to the batch for later execution, on fail, runs the request immediately </param>
		/// <param name="cdsServiceClient">Connected CDS Service Client</param>
		/// <param name="bypassPluginExecution">Adds the bypass plugin behavior to this request. Note: this will only apply if the caller has the prvBypassPlugins permission to bypass plugins.  If its attempted without the permission the request will fault.</param>
		/// <returns>Returns the ID of the closed ticket</returns>
		public static Guid CloseTroubleTicket(this CdsServiceClient cdsServiceClient, Guid ticketId, string subject, string description, Guid batchId = default(Guid), bool bypassPluginExecution = false)
		{
			// ONE OF THEASE SOULD BE MADE THE MASTER

			cdsServiceClient.logEntry.ResetLastError();  // Reset Last Error 
			if (cdsServiceClient._CdsService == null)
			{
				cdsServiceClient.logEntry.Log("Crm Service not initialized", TraceEventType.Error);
				return Guid.Empty;
			}

			if (ticketId == Guid.Empty)
				return Guid.Empty;

			// Create Incident Resolution Type

			Entity reso = new Entity("incidentresolution");

			Guid closeTicketId = Guid.NewGuid();
			reso.Attributes.Add("activityid", closeTicketId);
			reso.Attributes.Add("incidentid", new EntityReference("incident", ticketId));

			// NEED TO REWORK THIS WITH METAD DATA> 
			reso.Attributes.Add("statecode", new OptionSetValue(1));
			reso.Attributes.Add("statuscode", new OptionSetValue(2));
			reso.Attributes.Add("subject", subject);
			reso.Attributes.Add("description", description);

			// Set Close Time Stamp
			reso.Attributes.Add("actualend", DateTime.Now.ToString());

			// Get State close for Resolving a case 
			int defaultStateCodeForResolveCase = 1;
			CloseIncidentRequest req4 = new CloseIncidentRequest();
			req4.IncidentResolution = reso;
			req4.Status = new OptionSetValue(defaultStateCodeForResolveCase);


			if (cdsServiceClient.AddRequestToBatch(batchId, req4, "Calling Close Incident", "Request to Close Incident Queued",bypassPluginExecution))
				return Guid.Empty;

			CloseIncidentResponse resp4 = (CloseIncidentResponse)cdsServiceClient.CdsCommand_Execute(req4, "Closing a Case in CRM",bypassPluginExecution);
			if (resp4 != null)
				return closeTicketId;
			else
				return Guid.Empty;
		}



	}
}
