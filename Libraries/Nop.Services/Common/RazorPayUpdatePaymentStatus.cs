using Nop.Services.Logging;
using Nop.Services.Messages;
using Nop.Services.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Services.Common
{
    public partial class RazorPayUpdatePaymentStatus : ITask
    {
        private readonly IQueuedEmailService _queuedEmailService;
        private readonly IEmailSender _emailSender;
        private readonly ILogger _logger;

        public RazorPayUpdatePaymentStatus(IQueuedEmailService queuedEmailService,
            IEmailSender emailSender, ILogger logger)
        {
            this._queuedEmailService = queuedEmailService;
            this._emailSender = emailSender;
            this._logger = logger;
        }

        public virtual void Execute()
        {
            #region commented code
            //var maxTries = 3;
            //var queuedEmails = _queuedEmailService.SearchEmails(null, null, null, null,
            //    true, true, maxTries, false, 0, 500);
            //foreach (var queuedEmail in queuedEmails)
            //{
            //    var bcc = String.IsNullOrWhiteSpace(queuedEmail.Bcc)
            //                ? null
            //                : queuedEmail.Bcc.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            //    var cc = String.IsNullOrWhiteSpace(queuedEmail.CC)
            //                ? null
            //                : queuedEmail.CC.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);

            //    try
            //    {
            //        _emailSender.SendEmail(queuedEmail.EmailAccount,
            //            queuedEmail.Subject,
            //            queuedEmail.Body,
            //           queuedEmail.From,
            //           queuedEmail.FromName,
            //           queuedEmail.To,
            //           queuedEmail.ToName,
            //           queuedEmail.ReplyTo,
            //           queuedEmail.ReplyToName,
            //           bcc,
            //           cc,
            //           queuedEmail.AttachmentFilePath,
            //           queuedEmail.AttachmentFileName,
            //           queuedEmail.AttachedDownloadId);

            //        queuedEmail.SentOnUtc = DateTime.UtcNow;
            //    }
            //    catch (Exception exc)
            //    {
            //        _logger.Error(string.Format("Error sending e-mail. {0}", exc.Message), exc);
            //    }
            //    finally
            //    {
            //        queuedEmail.SentTries = queuedEmail.SentTries + 1;
            //        _queuedEmailService.UpdateQueuedEmail(queuedEmail);
            //    }
            //}
            #endregion
        }
    }
}
