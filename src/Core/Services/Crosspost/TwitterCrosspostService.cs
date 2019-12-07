using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Core.Logging;
using Core.Repositories;
using DAL;
using Serilog.Events;
using Tweetinvi;

namespace Core.Services.Crosspost
{
    public class TwitterCrosspostService : ICrossPostService
    {
        private static readonly Semaphore _semaphore = new Semaphore(1, 1);
        
        private readonly ISocialRepository _socialRepository;
        private readonly ILogger _logger;
        
        private const int MaxTweetLength = 277;

        public TwitterCrosspostService(
            ISocialRepository socialRepository,
            ILogger logger)
        {
            _logger = logger;
            _socialRepository = socialRepository;
        }

        public async Task Send(int categoryId, string comment, string link, IReadOnlyCollection<string> tags)
        {
            var accounts = await _socialRepository.GetTwitterAccountsChannels(categoryId);
            
            var tag = string.Join(" ", tags);
            var maxMessageLength = MaxTweetLength - link.Length - tag.Length;
            var message = Substring(comment, maxMessageLength);

            var text = $"{message} {tag} {link}";

            foreach (var account in accounts)
            {
                try
                {
                    _semaphore.WaitOne();
                    
                    Auth.SetUserCredentials(
                        account.ConsumerKey,
                        account.ConsumerSecret,
                        account.AccessToken,
                        account.AccessTokenSecret);

                    Tweet.PublishTweet(text);

                    _logger.Write(LogEventLevel.Information, $"Message was sent to Twitter channel `{account.Name}`: `{comment}` `{link}` Category: `{categoryId}`");
                }
                catch (Exception ex)
                {
                    _logger.Write(LogEventLevel.Error, "Error in TwitterCrosspostManager.Send", ex);
                }
                finally
                {
                    _semaphore.Release();    
                }
            }
        }


        private static string Substring(string text, int length)
        {
            if (string.IsNullOrWhiteSpace(text))
                return string.Empty;

            return text.Length <= length ? text : $"{text.Substring(0, length - 4)}... ";
        }

        public async Task<IReadOnlyCollection<TwitterAccount>> GetAccounts()
        {
            return await _socialRepository.GetTwitterAccounts();
        }
    }
}