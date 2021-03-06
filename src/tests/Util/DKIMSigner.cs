﻿using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;
using McGiv.AWS.SES.Commands;
using McGiv.AWS.SES.DKIM;
using McGiv.AWS.SES.Util;
using NUnit.Framework;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Security;

namespace McGiv.AWS.SES.Tests.Util
{

	

	public class Email
	{
		public Email()
		{
			this.HeaderLines = new List<string>();
			this.BodyLines = new List<string>();
		}

		public string Headers;
		public string Body;

		public List<String> HeaderLines;
		public List<String> BodyLines;
	}

	/// <summary>
	/// see https://forums.aws.amazon.com/thread.jspa?messageID=232009&#232009
	/// </summary>
	[TestFixture]
	public class DKIMSigner
	{
		private readonly CommandRequestBuilder _builder = new CommandRequestBuilder(new RequestSigner(Helper.GetCredentials()));

		public DKIMSigner()
		{

			this.Domain = "nimtug.org";
			this.Encoding = System.Text.Encoding.ASCII;
			this.BodyCanonicalization = CanonicalizationAlgorithm.Relaxed;
			this.HeaderCanonicalization = CanonicalizationAlgorithm.Relaxed;
			this.Selector = "testing123";
			this.PrivateKey = @"-----BEGIN RSA PRIVATE KEY-----
MIICWwIBAAKBgQC7tQiBtxdeRdH8XFGlyxf8qjJoyOJVfJrCbvHJVJG+ZiGzemZz
HvhurYEAj/2N9eL2qQWN3PzdJX4s7cFt2Wvdmj/MOGmIwPP6x2224ooSUp7FAeG3
H/ODMoAqDyXsdIC8mTj7YKpKCUki7/m3VDACSiOhfNxeAkykalHG/bjUuwIDAQAB
AoGAVGFkSpOY8KsoY27I0WQEC3QjJxGvFUjndSJUlPHsdpAI9FrAtV2lxnM+u5b/
H5L6jXGb6pL+JRfWqbHTs2L65qRlSnv9S+priPgryXHY/cORtBNgdMwNfjMJhPxE
CY3vw4KBL2L0IxRqoZeVsmu8g1cLKMrLVRXAcF7rWJnR8yECQQDzd1+iMTKu1c+4
FzZn17dscCVbeWBvvIXvkNRpa1dnadBEGqBxIYQMLUeqAsGAyF1aHZUWruZfOFeu
4Qlqo9MRAkEAxV7QR0l2xYresNwNrWnK9lB7F8HCogxoplp3dbyd++8NVSxhhWWQ
PVgBPozRiycSgbuUzEKeVtDrr2zi9ryTCwJAZZH+nr6ho1jl4Komc2oGRsH+g8v+
VH807T3hr90tSKJXVaI6HxhZa28Uf7PIoH52m5rN0PnEeCMcSYPulsOj0QJAMUXH
R1Suwwg1Kf/1pio4Eh/ravXjSiNA6O7CzfDFnASE1pOa0PuW88mJnfz3vv6FH0Ae
GJQ1BUVo4UWUr7ZKGwJANCEGFsILGGdMaZhmuZcUqphoR7um0Sa7sQQHiUzQuJZe
6GaqmpEox9En9rUPIkRog8wpvWPtb3njTqmTbk8VMg==
-----END RSA PRIVATE KEY-----";


		}
		public string Selector { get; set; }
		public string Domain { get; set; }
		public string PrivateKey { get; set; }
		public Encoding Encoding { get; set; }
		public CanonicalizationAlgorithm HeaderCanonicalization { get; set; }
		public CanonicalizationAlgorithm BodyCanonicalization { get; set; }

		private readonly HashAlgorithm _hashAlgorithm = new SHA256Managed();

		public string SignEmail(NameValueCollection headers, string body, params string[] headersToSign)
		{
			return null;

		}


		//public string CanonicalizationHeaders(NameValueCollection headers, CanonicalizationAlgorithm type)
		//{
		//    switch (type)
		//    {

		//        case CanonicalizationAlgorithm.Relaxed:
		//            {
		//                var catHeaders = new StringBuilder();

		//                foreach (string header in headers.Keys)
		//                {
		//                    catHeaders.Append(header.ToLower());
		//                    catHeaders.Append(':');
		//                    catHeaders.AppendLine(headers[header]);
		//                }

		//                break;
		//            }
		//    }
		//}









		public string SignBody(string body)
		{

			return Convert.ToBase64String(_hashAlgorithm.ComputeHash(Encoding.GetBytes(body)));

		}



		[TestCase(@"

----boundary_0_999a53c3-fb74-4ccc-ab24-701a00fcfdb5
Content-Type: text/plain; charset=utf-8
Content-Transfer-Encoding: base64

dGhpcyBpcyBzb21lIHRleHQ=
----boundary_0_999a53c3-fb74-4ccc-ab24-701a00fcfdb5
Content-Type: text/html; charset=utf-8
Content-Transfer-Encoding: base64

PHA+dGhpcyBpcyBzb21lIGh0bWw8L3A+PHA+YXNkYXNkIGFzZGFzZDwvcD4=
----boundary_0_999a53c3-fb74-4ccc-ab24-701a00fcfdb5--


", "ovb8JMVS0WYj0GchAvY1C53hBCO9D4eu8EvnP6m5llw=", CanonicalizationAlgorithm.Relaxed)]

		[TestCase(@"a=0D=0Ab  =0D=0Ac       =0D=0Ad=0D=0A=0D=0A=0D=0A", "", CanonicalizationAlgorithm.Relaxed)]
		public void SignBodyTests(string body, string hash, CanonicalizationAlgorithm type)
		{
			string cb = Canonicalization.CanonicalizationBody(body, type);

			Console.WriteLine("-- start --");
			Console.WriteLine(body);
			Console.WriteLine("-- end --");


			Console.WriteLine("-- start --");
			Console.WriteLine(cb);
			Console.WriteLine("-- end --");


			Assert.AreEqual(hash, SignBody(cb));
		}


		[TestCase("a=0D=0Ab'20'=0D=0Ac'20'=0D=0Ad=0D=0A=0D=0A=0D=0A'0D''0A'", "DBHADyQJfcQsPAmrIibuplmOld+95Rlov0bB55eU7t4=")]
		[TestCase("a=0D=0Ab =0D=0Ac =0D=0Ad=0D=0A=0D=0A=0D=0A'0D''0A'", "DBHADyQJfcQsPAmrIibuplmOld+95Rlov0bB55eU7t4=")]
		public void TestHash(string body, string hash)
		{
			Assert.AreEqual(hash, this.SignBody(body));
		}

		string GenerateSignature(Email email, params string[] signHeaders)
		{
			

			// timestamp  - seconds since 00:00:00 on January 1, 1970 UTC
			TimeSpan t = DateTime.Now.ToUniversalTime() -
						 DateTime.SpecifyKind(DateTime.Parse("00:00:00 January 1, 1970"), DateTimeKind.Utc);


			var signatureValue = new StringBuilder();

			signatureValue.Append("v=1; ");
			signatureValue.Append("a=rsa-sha256; ");

			// Canonicalization
			signatureValue.Append("c=");
			signatureValue.Append(this.HeaderCanonicalization.ToString().ToLower());
			signatureValue.Append('/');
			signatureValue.Append(this.BodyCanonicalization.ToString().ToLower());
			signatureValue.Append("; ");


			signatureValue.Append("q=dns/txt; ");


			// signing domain
			signatureValue.Append("d=");
			signatureValue.Append(this.Domain);
			signatureValue.Append("; ");

			// selector
			signatureValue.Append("s=");
			signatureValue.Append(this.Selector);
			signatureValue.Append("; ");

			// time sent
			signatureValue.Append("t=");
			signatureValue.Append((int)t.TotalSeconds);
			signatureValue.Append("; ");


			// hash of body
			signatureValue.Append("bh=");
			signatureValue.Append(SignBody(Canonicalization.CanonicalizationBody(email.Body, this.BodyCanonicalization)));
			signatureValue.Append("; ");

			// headers to be signed
			signatureValue.Append("h=");
			//sb.Append(string.Join(":", headers));
			foreach (var header in signHeaders)
			{
				signatureValue.Append(header);
				signatureValue.Append(':');
			}
			signatureValue.Length--;
			signatureValue.Append("; ");

			signatureValue.Append("b=");


			Console.WriteLine();
			Console.WriteLine("---- start sig ----");
			Console.WriteLine(signatureValue);
			Console.WriteLine("---- start sig ----");




			var catHeaders = new StringBuilder();

			catHeaders.Append(Canonicalization.CanonicalizationHeaders(email.Headers, this.HeaderCanonicalization));
			//foreach (var header in signHeaders)
			//{
			//    catHeaders.Append(header.ToLower());
			//    catHeaders.Append(':');
			//    catHeaders.AppendLine(headers[header]);
			//}

			catHeaders.Append("dkim-signature:");
			catHeaders.Append(signatureValue);


			Console.WriteLine("---- can sig ----");
			Console.WriteLine(catHeaders);
			Console.WriteLine("---- can end ----");

			using (TextReader reader = new StringReader(this.PrivateKey))
			{
				var r = new PemReader(reader);
				var o = (AsymmetricCipherKeyPair)r.ReadObject();
				byte[] plaintext = this.Encoding.GetBytes(catHeaders.ToString());
				ISigner sig = SignerUtilities.GetSigner("SHA256WithRSAEncryption");
				sig.Init(true, o.Private);
				sig.BlockUpdate(plaintext, 0, plaintext.Length);
				byte[] signature = sig.GenerateSignature();
				signatureValue.Append(Convert.ToBase64String(signature));
			}

			return signatureValue.ToString();

		}



		public Email ParseEmail(byte[] data)
		{
			var email = new Email();
			bool isHeader = true;
			using (var reader = new StreamReader(new MemoryStream(data)))
			{
				string line;
				while ((line = reader.ReadLine()) != null)
				{
					if (isHeader)
					{
						if (line == string.Empty)
						{
							isHeader = false;
							continue;
						}

						email.HeaderLines.Add(line);
					}
					else
					{
						email.BodyLines.Add(line);
					}
				}
			}

			email.Body = string.Join(Environment.NewLine, email.BodyLines);
			email.Headers = string.Join(Environment.NewLine, email.HeaderLines) + Environment.NewLine;

			return email;
		}


		public static Email ParseEmail(string data)
		{
			var email = new Email();
			bool isHeader = true;
			using (var reader = new StringReader(data))
			{
				string line;
				while ((line = reader.ReadLine()) != null)
				{
					if (isHeader)
					{
						if (line == string.Empty)
						{
							isHeader = false;
							continue;
						}

						email.HeaderLines.Add(line);
					}
					else
					{
						email.BodyLines.Add(line);
					}
				}
			}

			email.Body = string.Join(Environment.NewLine, email.BodyLines);
			email.Headers = string.Join(Environment.NewLine, email.HeaderLines) + Environment.NewLine;

			return email;
		}



		[Test]
		public void Overall()
		{
			var data = RawEmailGenerator.SendRawEmail(this.Encoding, "admin@nimtug.org", "check-auth@verifier.port25.com", "test",
			                                          @"a
b  
c	
d


", null);

			var email = this.ParseEmail(data);

			

		}
		[Test]
		public void GenerateEmail()
		{
//            var data = RawEmailGenerator.SendRawEmail(this.Encoding, "admin@nimtug.org", "damien@mcgiv.com"/*"check-auth@verifier.port25.com"*/, "test", @"this is some text
//xfgdfgjkdfg dfgkjdgf kdjfg dfgkjdfg kdfjhd fgkjdfg dfgkjhfdgkjdhfgkjdhfgk dfkjgdhf gkjdhfg sdfkjshdfkjshdfjhsdfjhsdfhjgsdf
//and a seconf line",
//                         @"<p>this is some html</p>
//xfgdfgjkdfg dfgkjdgf kdjfg dfgkjdfg kdfjhd fgkjdfg dfgkjhfdgkjdhfgkjdhfgk dfkjgdhf gkjdhfg sdfkjshdfkjshdfjhsdfjhsdfhjgsdf
//<p>asdasd asdasd</p>");


			var data = RawEmailGenerator.SendRawEmail(this.Encoding, "admin@nimtug.org", "check-auth@verifier.port25.com", "test", @"abc",
			 null);


			var email = ParseEmail(data);

			var sig = GenerateSignature(email, "From", "To", "Subject");

			var text = "DKIM-Signature: " + sig + Environment.NewLine + new StreamReader(new MemoryStream(data)).ReadToEnd();// Environment.NewLine + email.Headers + Environment.NewLine + email.Body;

			Console.WriteLine("---- start ----");
			Console.WriteLine(text);
			Console.WriteLine("---- end ----");

			var cmd = new SendRawEmailCommand
			{
				

				RawData = Convert.ToBase64String( this.Encoding.GetBytes(text) )
			};

			Console.WriteLine(cmd.RawData);

			var cp = new CommandProcessor(_builder);

			var resp = cp.Process(cmd, new SendRawEmailCommandResponseParser());


			Console.WriteLine(resp.Command + " : ID " + resp.RequestID);
		}

		[Test]
		public void Sign()
		{
			string smtp = "DEEP";
			string from = "test123@nimtug.org";
			string subject = "dkim test email";
			string to = "check-auth@verifier.port25.com";
			string body = "This is the body of the message." + Environment.NewLine + "This is the second line";
			string base64privatekey =
				@"-----BEGIN RSA PRIVATE KEY-----
MIICWwIBAAKBgQC7tQiBtxdeRdH8XFGlyxf8qjJoyOJVfJrCbvHJVJG+ZiGzemZz
HvhurYEAj/2N9eL2qQWN3PzdJX4s7cFt2Wvdmj/MOGmIwPP6x2224ooSUp7FAeG3
H/ODMoAqDyXsdIC8mTj7YKpKCUki7/m3VDACSiOhfNxeAkykalHG/bjUuwIDAQAB
AoGAVGFkSpOY8KsoY27I0WQEC3QjJxGvFUjndSJUlPHsdpAI9FrAtV2lxnM+u5b/
H5L6jXGb6pL+JRfWqbHTs2L65qRlSnv9S+priPgryXHY/cORtBNgdMwNfjMJhPxE
CY3vw4KBL2L0IxRqoZeVsmu8g1cLKMrLVRXAcF7rWJnR8yECQQDzd1+iMTKu1c+4
FzZn17dscCVbeWBvvIXvkNRpa1dnadBEGqBxIYQMLUeqAsGAyF1aHZUWruZfOFeu
4Qlqo9MRAkEAxV7QR0l2xYresNwNrWnK9lB7F8HCogxoplp3dbyd++8NVSxhhWWQ
PVgBPozRiycSgbuUzEKeVtDrr2zi9ryTCwJAZZH+nr6ho1jl4Komc2oGRsH+g8v+
VH807T3hr90tSKJXVaI6HxhZa28Uf7PIoH52m5rN0PnEeCMcSYPulsOj0QJAMUXH
R1Suwwg1Kf/1pio4Eh/ravXjSiNA6O7CzfDFnASE1pOa0PuW88mJnfz3vv6FH0Ae
GJQ1BUVo4UWUr7ZKGwJANCEGFsILGGdMaZhmuZcUqphoR7um0Sa7sQQHiUzQuJZe
6GaqmpEox9En9rUPIkRog8wpvWPtb3njTqmTbk8VMg==
-----END RSA PRIVATE KEY-----";

			HashAlgorithm hash = new SHA256Managed();
			// HACK!! simulate the quoted-printable encoding SmtpClient will use
			string hashBody = body.Replace(Environment.NewLine, "=0D=0A") + Environment.NewLine;
			byte[] bodyBytes = Encoding.ASCII.GetBytes(hashBody);
			string hashout = Convert.ToBase64String(hash.ComputeHash(bodyBytes));
			// timestamp  - seconds since 00:00:00 on January 1, 1970 UTC
			TimeSpan t = DateTime.Now.ToUniversalTime() -
						 DateTime.SpecifyKind(DateTime.Parse("00:00:00 January 1, 1970"), DateTimeKind.Utc);

			string signatureHeader = "v=1; " +
									 "a=rsa-sha256; " +
									 "c=relaxed/relaxed; " +
									 "q=dns/txt; " +
									 "d=nimtug.org; " +
									 "s=testing123; " +
									 "t=" + Convert.ToInt64(t.TotalSeconds) + "; " +
									 "bh=" + hashout + "; " + // hash of message body
									 "h=From:To:Subject:Content-Type:Content-Transfer-Encoding; " +
									 "b=";

			string canonicalizedHeaders =
				"from:" + from + Environment.NewLine +
				"to:" + to + Environment.NewLine +
				"subject:" + subject + Environment.NewLine +
				@"content-type:text/plain; charset=us-ascii
content-transfer-encoding:quoted-printable
dkim-signature:" +
				signatureHeader;

			TextReader reader = new StringReader(base64privatekey);
			var r = new PemReader(reader);
			var o = r.ReadObject() as AsymmetricCipherKeyPair;
			byte[] plaintext = Encoding.ASCII.GetBytes(canonicalizedHeaders);
			ISigner sig = SignerUtilities.GetSigner("SHA256WithRSAEncryption");
			sig.Init(true, o.Private);
			sig.BlockUpdate(plaintext, 0, plaintext.Length);
			byte[] signature = sig.GenerateSignature();
			signatureHeader += Convert.ToBase64String(signature);

			var message = new MailMessage();
			message.From = new MailAddress(from);
			message.To.Add(new MailAddress(to));
			message.Subject = subject;
			message.Body = body;

			message.Headers.Add("DKIM-Signature", signatureHeader);
			message.Headers.Add("Message-Id", "123123123@nimtug.org");
			var client = new SmtpClient(smtp);
			client.Send(message);
			Console.Write("sent to: " + to);
		}
	}
}