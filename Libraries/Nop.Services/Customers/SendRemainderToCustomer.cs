using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Messages;
using Nop.Services.Events;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Services.Customers
{
    public partial class SendRemainderToCustomer : ITask
    {
        private readonly ICustomerService _customerService;
        private readonly CustomerSettings _customerSettings;
        private readonly IMessageTemplateService _messageTemplateService;
        private readonly IStoreContext _storeContext;
        private readonly IEmailAccountService _emailAccountService;
        private readonly EmailAccountSettings _emailAccountSettings;
        private readonly IWorkContext _workContext;
        private readonly IMessageTokenProvider _messageTokenProvider;
        private readonly ITokenizer _tokenizer;
        private readonly IQueuedEmailService _queuedEmailService;
        private readonly IEventPublisher _eventPublisher;

        public SendRemainderToCustomer(ICustomerService customerService
            , CustomerSettings customerSettings
            , IMessageTemplateService messageTemplateService
            , IStoreContext storeContext
            , IEmailAccountService emailAccountService
            , EmailAccountSettings emailAccountSettings
            , IWorkContext workContext
            , IMessageTokenProvider messageTokenProvider
            , ITokenizer tokenizer
            , IQueuedEmailService queuedEmailService,
            IEventPublisher eventPublisher)
        {
            this._customerService = customerService;
            this._customerSettings = customerSettings;
            this._messageTemplateService = messageTemplateService;
            this._storeContext = storeContext;
            this._emailAccountService = emailAccountService;
            this._emailAccountSettings = emailAccountSettings;
            this._workContext = workContext;
            this._messageTokenProvider = messageTokenProvider;
            this._tokenizer = tokenizer;
            this._queuedEmailService = queuedEmailService;
            this._eventPublisher = eventPublisher;
        }
        public void Execute()
        {
            List<Customer> customersWithItemInCart = _customerService.GetAllCustomersWithItemsInCart().ToList();
            if (customersWithItemInCart != null && customersWithItemInCart.Count > 0)
            {
                var store = _storeContext.CurrentStore;
                //languageId = EnsureLanguageIsActive(_workContext.WorkingLanguage.Id, store.Id);
                var messageTemplate = GetActiveMessageTemplate("CustomerWithItemInCart.EmailReminder", store.Id);
                if (messageTemplate != null)
                {
                    var emailAccount = GetEmailAccountOfMessageTemplate(messageTemplate, _workContext.WorkingLanguage.Id);
                    //tokens
                    var tokens = new List<Token>();
                    _messageTokenProvider.AddStoreTokens(tokens, store, emailAccount);
                    foreach (Customer customer in customersWithItemInCart)
                    {
                        _messageTokenProvider.AddCustomerTokens(tokens, customer);
                        _eventPublisher.MessageTokensAdded(messageTemplate, tokens);

                        SendNotification(messageTemplate, emailAccount, _workContext.WorkingLanguage.Id, tokens
                            , customer.Email, customer.GetFullName());
                    }
                }
            }
        }
        protected virtual MessageTemplate GetActiveMessageTemplate(string messageTemplateName, int storeId)
        {
            var messageTemplate = _messageTemplateService.GetMessageTemplateByName(messageTemplateName, storeId);

            //no template found
            if (messageTemplate == null)
                return null;

            //ensure it's active
            var isActive = messageTemplate.IsActive;
            if (!isActive)
                return null;

            return messageTemplate;
        }

        protected virtual EmailAccount GetEmailAccountOfMessageTemplate(MessageTemplate messageTemplate, int languageId)
        {
            var emailAccountId = messageTemplate.GetLocalized(mt => mt.EmailAccountId, languageId);
            //some 0 validation (for localizable "Email account" dropdownlist which saves 0 if "Standard" value is chosen)
            if (emailAccountId == 0)
                emailAccountId = messageTemplate.EmailAccountId;

            var emailAccount = _emailAccountService.GetEmailAccountById(emailAccountId);
            if (emailAccount == null)
                emailAccount = _emailAccountService.GetEmailAccountById(_emailAccountSettings.DefaultEmailAccountId);
            if (emailAccount == null)
                emailAccount = _emailAccountService.GetAllEmailAccounts().FirstOrDefault();
            return emailAccount;

        }

        public virtual int SendNotification(MessageTemplate messageTemplate,
           EmailAccount emailAccount, int languageId, IEnumerable<Token> tokens,
           string toEmailAddress, string toName,
           string attachmentFilePath = null, string attachmentFileName = null,
           string replyToEmailAddress = null, string replyToName = null,
           string fromEmail = null, string fromName = null, string subject = null)
        {
            if (messageTemplate == null)
                throw new ArgumentNullException("messageTemplate");

            if (emailAccount == null)
                throw new ArgumentNullException("emailAccount");

            //retrieve localized message template data
            var bcc = messageTemplate.GetLocalized(mt => mt.BccEmailAddresses, languageId);
            if (String.IsNullOrEmpty(subject))
                subject = messageTemplate.GetLocalized(mt => mt.Subject, languageId);
            var body = messageTemplate.GetLocalized(mt => mt.Body, languageId);

            //Replace subject and body tokens 
            var subjectReplaced = _tokenizer.Replace(subject, tokens, false);
            var bodyReplaced = _tokenizer.Replace(body, tokens, true);

            //limit name length
            toName = CommonHelper.EnsureMaximumLength(toName, 300);

            var email = new QueuedEmail
            {
                Priority = QueuedEmailPriority.High,
                From = !string.IsNullOrEmpty(fromEmail) ? fromEmail : emailAccount.Email,
                FromName = !string.IsNullOrEmpty(fromName) ? fromName : emailAccount.DisplayName,
                To = toEmailAddress,
                ToName = toName,
                ReplyTo = replyToEmailAddress,
                ReplyToName = replyToName,
                CC = string.Empty,
                Bcc = bcc,
                Subject = subjectReplaced,
                Body = bodyReplaced,
                AttachmentFilePath = attachmentFilePath,
                AttachmentFileName = attachmentFileName,
                AttachedDownloadId = messageTemplate.AttachedDownloadId,
                CreatedOnUtc = DateTime.UtcNow,
                EmailAccountId = emailAccount.Id,
                DontSendBeforeDateUtc = !messageTemplate.DelayBeforeSend.HasValue ? null
                    : (DateTime?)(DateTime.UtcNow + TimeSpan.FromHours(messageTemplate.DelayPeriod.ToHours(messageTemplate.DelayBeforeSend.Value)))
            };

            _queuedEmailService.InsertQueuedEmail(email);
            return email.Id;
        }
    }
}
