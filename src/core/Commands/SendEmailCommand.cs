﻿using System;
using System.Collections.Generic;
using System.Xml;

namespace McGiv.AWS.SES
{
	/// <summary>
	/// see http://docs.amazonwebservices.com/ses/latest/APIReference/index.html?API_SendEmail.html
	/// </summary>
	[Serializable]
	public class SendEmailCommand : ICommand
	{
		public SendEmailCommand()
		{
			Destination = new Destination();
			Message = new Message();
		}

		/// <summary>
		/// Sender's emails address
		/// </summary>
		public string Source { get; set; }

		public Destination Destination { get; set; }


		public Message Message { get; set; }

		#region ICommand Members

		public string Action
		{
			get { return "SendEmail"; }
		}

		public Dictionary<string, string> GetData()
		{
			var data = new Dictionary<string, string> {{"Source", Source}};

			int i = 1;
			foreach (string email in Destination.ToAddresses)
			{
				data.Add("Destination.ToAddresses.member." + (i++), email);
			}
			//data.Add("Destination.ToAddresses", string.Join("; ", this.Destination.ToAddresses));
			//data.Add("Destination.CcAddresses", string.Join("; ", this.Destination.CcAddresses));
			//data.Add("Destination.BccAddresses", string.Join("; ", this.Destination.BccAddresses));

			//data.Add("Message.Subject.Charset", this.Message.Subject.Charset);
			data.Add("Message.Subject.Data", Message.Subject.Data);
			data.Add("Message.Body.Html.Data", Message.Body.Html.Data);
			data.Add("Message.Body.Text.Data", Message.Body.Text.Data);

			return data;
		}

		#endregion
	}



	[Serializable]
	public class SendEmailResponse : Response
	{
		public string MessageID { get; set; }
	}



	public class SendEmailResponseParser : ResponseParser<SendEmailResponse>
	{
		// example
		/*
<SendEmailResponse xmlns="http://ses.amazonaws.com/doc/2010-12-01/">
  <SendEmailResult>
    <MessageId>0000012dde363baf-4b325872-d719-4a10-b7ff-df1c8f7ad435-000000</MessageId>
  </SendEmailResult>
  <ResponseMetadata>
    <RequestId>39c456cc-2d8a-11e0-9e85-4157ce6dd0c8</RequestId>
  </ResponseMetadata>
</SendEmailResponse>

		 * */

		#region ICommandResponseParser<SendQuote> Members

		public SendEmailResponseParser()
			: base("SendEmailResponse")
		{
		}

		protected override void InnerParse(XmlReader reader, SendEmailResponse response)
		{

			reader.ReadStartElement("SendEmailResult");
			while (reader.Read())
			{
				if (reader.NodeType != XmlNodeType.Element)
				{
					continue;
				}

				switch (reader.Name)
				{
					case "MessageId":
						{
							response.MessageID = GetNextValue(reader);
							break;
						}
					case "ResponseMetadata":
						{
							return;
						}

				}
			}
		}



		#endregion

		
	}

}