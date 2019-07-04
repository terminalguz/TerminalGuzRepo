// Copyright © 2019 Transeric Solutions. All rights reserved.
// Author: Eric David Lynch
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using Octokit;


namespace GitHubApiSnippets
{
	public class Program
	{
		private static readonly string GitHubIdentity = Assembly
			.GetEntryAssembly()
			.GetCustomAttribute<AssemblyProductAttribute>()
			.Product;

		public static void Main(string[] args)
		{
			var productInformation = new ProductHeaderValue(GitHubIdentity);

			if (!TryGetClient(args, productInformation, out GitHubClient client))
				return;

			TestFeature(client)
				.GetAwaiter()
				.GetResult();
		}

		private static async Task<string> CreateOAuthToken(string clientId, string clientSecret, string authenticationCode)
		{
			var client = new GitHubClient(new ProductHeaderValue(GitHubIdentity));

			OauthToken tokenInfo = await client.Oauth.CreateAccessToken(
				new OauthTokenRequest(clientId, clientSecret, authenticationCode));

			string token = tokenInfo.AccessToken;
			return token;
		}

		private static GitHubClient GetClient(ProductHeaderValue productInformation)
		{
			var client = new GitHubClient(productInformation);
			return client;
		}

		private static GitHubClient GetClient(ProductHeaderValue productInformation,
			string token)
		{
			var credentials = new Credentials(token);

			var client = new GitHubClient(productInformation) { Credentials = credentials };

			return client;
		}

		private static GitHubClient GetClient(ProductHeaderValue productInformation,
			string username, string password)
		{
			var credentials = new Credentials(username, password, AuthenticationType.Basic);

			var client = new GitHubClient(productInformation) { Credentials = credentials };

			return client;
		}

		private static GitHubClient GetEnterpriseClient(ProductHeaderValue productInformation,
			Credentials credentials, string enterpriseUrl)
		{
			var client = new GitHubClient(productInformation, new Uri(enterpriseUrl))
			{
				Credentials = credentials
			};

			return client;
		}

		private static async Task CodeExamples(GitHubClient client)
		{
			SearchCodeResult result = await client.Search.SearchCode(
				new SearchCodeRequest("issue")
				{
					In = new CodeInQualifier[] { CodeInQualifier.Path },
					Language = Language.CSharp,
					Repos = new RepositoryCollection { "octokit/octokit.net" }
				});
			Console.WriteLine($"Search.SearchCode (Simple Search): TotalCode={result.TotalCount}");

			await CodeAllFieldExample(client);
		}

		private static async Task CodePagingExample(GitHubClient client)
		{
			const int MaximumSearchItems = 1000;

			var request = new SearchCodeRequest
			{
				Language = Language.CSharp,
				Repos = new RepositoryCollection { "octokit/octokit.net" }
			};

			int maximumPage = MaximumSearchItems / request.PerPage;
			for (request.Page = 1; request.Page <= maximumPage; request.Page++)
			{
				SearchCodeResult result = await client.Search.SearchCode(request);
				if (request.Page == 1)
					Console.WriteLine($"Search.SearchCode (Paging): TotalCount={result.TotalCount}");

				foreach (SearchCode code in result.Items)
					Console.WriteLine($"  {code.Path}");

				if (result.IncompleteResults || result.Items.Count < request.PerPage)
					break;
			}
		}

		private static async Task CodeAllFieldExample(GitHubClient client)
		{
			int fromNumber = 100;
			int toNumber = 1000;

			string extension = ".cs";
			string fileName = "IssueCommentPayload";
			string organization = "octokit";
			string path = "Octokit/Models/Response/ActivityPayloads";
			string repository = "octokit/octokit.net";
			string term = "issue";
			string user = "octokit";

			var request = new SearchCodeRequest(term)
			{
				Extension = extension,
				FileName = fileName,
				Forks = false,
				In = new CodeInQualifier[] { CodeInQualifier.Path },
				Language = Language.CSharp,
				Order = SortDirection.Descending,
				Organization = organization,
				Path = path,
				Repos = new RepositoryCollection { repository },
				Size = new Range(fromNumber, toNumber),
				SortField = CodeSearchSort.Indexed,
				User = user
			};

			SearchCodeResult result = await client.Search.SearchCode(request);
			Console.WriteLine($"Search.SearchCode (All Fields): TotalCount={result.TotalCount}");
		}

		private static async Task IssueExamples(GitHubClient client)
		{
			Issue issue = await client.Issue.Get("octokit", "octokit.net", 1);
			Console.WriteLine($"Issue.Get: Id={issue.Id}, Title={issue.Title}");

			SearchIssuesResult result = await client.Search.SearchIssues(
				new SearchIssuesRequest("reviews")
				{
					In = new IssueInQualifier[] { IssueInQualifier.Title },
					Repos = new RepositoryCollection { "octokit/octokit.net" }
				});
			Console.WriteLine($"Search.SearchIssues (Simple Search): TotalCount={result.TotalCount}");

			await IssueAllFieldsExample(client);
		}

		private static async Task IssueAllFieldsExample(GitHubClient client)
		{
			var fromDate = new DateTime(2018, 3, 17);
			var toDate = new DateTime(2019, 3, 17);

			int fromNumber = 1;
			int toNumber = 10;

			string branch = "master";
			string excludedBranch = "other";
			string excludedLabel = "wth";
			string excludedMilestone = "Nothing Done";
			string excludedUser = "somebody";
			string label = "up-for-grabs";
			string milestone = "API Cleanup";
			string repository = "octokit/octokit.net";
			string term = "bug";
			string user = "octokit";

			var request = new SearchIssuesRequest(term)
			{
				Archived = true,
				Assignee = user,
				Author = user,
				Base = branch,
				Closed = new DateRange(fromDate, toDate),
				Commenter = user,
				Comments = new Range(fromNumber, toNumber),
				Created = new DateRange(fromDate, SearchQualifierOperator.GreaterThan),
				Exclusions = new SearchIssuesRequestExclusions
				{
					Assignee = excludedUser,
					Author = excludedUser,
					Base = excludedBranch,
					Commenter = excludedUser,
					Head = branch,
					Involves = excludedUser,
					Labels = new string[] { excludedLabel },
					Language = Language.Ada,
					Mentions = excludedUser,
					Milestone = excludedMilestone,
					State = ItemState.Open,
					Status = CommitState.Error
				},
				Head = branch,
				In = new IssueInQualifier[] { IssueInQualifier.Title },
				Involves = user,
				Is = new IssueIsQualifier[] { IssueIsQualifier.Public },
				Labels = new string[] { label },
				Language = Language.CSharp,
				Mentions = user,
				Merged = new DateRange(toDate, SearchQualifierOperator.LessThan),
				Milestone = milestone,
				No = IssueNoMetadataQualifier.Assignee,
				Order = SortDirection.Descending,
				Repos = new RepositoryCollection() { repository },
				SortField = IssueSearchSort.Created,
				State = ItemState.Closed,
				Status = CommitState.Success,
				Type = IssueTypeQualifier.Issue,
				Updated = new DateRange(toDate, SearchQualifierOperator.LessThanOrEqualTo),
				User = user
			};

			SearchIssuesResult result = await client.Search.SearchIssues(request);
			Console.WriteLine($"Search.SearchIssues (All Fields): TotalCount={result.TotalCount}");
		}

		private static async Task LabelExamples(GitHubClient client)
		{
			Repository repository = await client.Repository.Get("octokit", "octokit.net");

			SearchLabelsResult result = await client.Search.SearchLabels(
				new SearchLabelsRequest("category", repository.Id));
			Console.WriteLine($"Search.SearchLabels (Simple Search): TotalCount={result.TotalCount}");

			await LabelAllFieldsExample(client, repository);
		}

		private static async Task LabelAllFieldsExample(GitHubClient client, Repository repository)
		{
			string term = "category";

			var request = new SearchLabelsRequest(term, repository.Id)
			{
				Order = SortDirection.Descending,
				SortField = LabelSearchSort.Created
			};

			SearchLabelsResult result = await client.Search.SearchLabels(request);
			Console.WriteLine($"Search.SearchLabels (All Fields): TotalCount={result.TotalCount}");
		}

		private static async Task RepositoryExamples(GitHubClient client)
		{
			Repository repository = await client.Repository.Get("octokit", "octokit.net");
			Console.WriteLine($"Repository.Get: Id={repository.Id}");

			Branch branch = await client.Repository.Branch.Get("octokit", "octokit.net", "master");
			Console.WriteLine($"Repository.Branch.Get: Name={branch.Name}");

			SearchRepositoryResult result = await client.Search.SearchRepo(
				new SearchRepositoriesRequest("octokit")
				{
					In = new InQualifier[] { InQualifier.Name }
				});
			Console.WriteLine($"Search.SearchRepo (Simple Search): TotalCount={result.TotalCount}");

			await RepositoryAllFieldsExample(client);
		}

		private static async Task RepositoryAllFieldsExample(GitHubClient client)
		{
			var fromDate = new DateTime(2012, 3, 17);
			var toDate = new DateTime(2019, 3, 17);

			int fromNumber = 1;
			int toNumber = 10000;

			string term = "octokit";
			string user = "octokit";

			var request = new SearchRepositoriesRequest(term)
			{
				Archived = false,
				Created = new DateRange(fromDate, toDate),
				Fork = ForkQualifier.IncludeForks,
				Forks = new Range(fromNumber, toNumber),
				In = new InQualifier[] { InQualifier.Name },
				Language = Language.CSharp,
				Order = SortDirection.Descending,
				Size = new Range(fromNumber, SearchQualifierOperator.GreaterThan),
				SortField = RepoSearchSort.Stars,
				Stars = new Range(fromNumber, SearchQualifierOperator.GreaterThanOrEqualTo),
				Updated = new DateRange(fromDate, SearchQualifierOperator.GreaterThan),
				User = user
			};

			SearchRepositoryResult result = await client.Search.SearchRepo(request);
			Console.WriteLine($"Search.SearchRepo (All Fields): TotalCount={result.TotalCount}");
		}

		private static async Task RepositoryForksExample(GitHubClient client)
		{
			IReadOnlyList<Repository> allForks = await client.Repository.Forks.GetAll(
				"octokit", "octokit.net");
			Console.WriteLine($"Repository.Forks.GetAll (All): {allForks.Count}");

			var options = new ApiOptions { PageCount = 1, PageSize = 100, StartPage = 1 };
			int maximumCount = options.PageCount.Value * options.PageSize.Value;
			Console.WriteLine("Repository.Forks.GetAll (Paging):");

			while (true)
			{
				IReadOnlyList<Repository> forks = await client.Repository.Forks.GetAll(
					"octokit", "octokit.net", options);
				foreach (Repository fork in forks)
					Console.WriteLine($"  {fork.Owner.Login}/{fork.Name}");

				if (forks.Count < maximumCount)
					break;

				options.StartPage++;
			}
		}

		private static async Task UserExamples(GitHubClient client)
		{
			User user = await client.User.Get("octokit");
			Console.WriteLine($"User.Get: Id={user.Id}");

			SearchUsersResult result = await client.Search.SearchUsers(
				new SearchUsersRequest("oct")
				{
					AccountType = AccountSearchType.User
				});
			Console.WriteLine($"Search.SearchUsers (Simple Search): TotalCount={result.TotalCount}");

			await UserAllFieldsExample(client);
		}

		private static async Task UserAllFieldsExample(GitHubClient client)
		{
			var fromDate = new DateTime(2001, 3, 17);

			int fromNumber = 1;
			int toNumber = 1000;

			string location = "Ontario";

			var request = new SearchUsersRequest("code")
			{
				AccountType = AccountSearchType.User,
				Created = new DateRange(fromDate, SearchQualifierOperator.GreaterThan),
				Followers = new Range(fromNumber, SearchQualifierOperator.GreaterThanOrEqualTo),
				In = new UserInQualifier[] { UserInQualifier.Username },
				Language = Language.CSharp,
				Location = location,
				Order = SortDirection.Descending,
				Repositories = new Range(fromNumber, toNumber),
				SortField = UsersSearchSort.Followers
			};

			var result = await client.Search.SearchUsers(request);
			Console.WriteLine($"Search.SearchUsers (All Fields): TotalCount={result.TotalCount}");
		}

		private static GitHubClient AuthenticateToken(string[] args, ProductHeaderValue productionInformation)
		{
			string token;
			if (args.Length > 1)
				token = args[1];
			else
			{
				Console.Write("OAuth Token? ");
				token = Console.ReadLine();
			}

			return GetClient(productionInformation, token);
		}

		private static GitHubClient AuthenticateBasic(string[] args, ProductHeaderValue productInformation)
		{
			string username;
			if (args.Length > 1)
				username = args[1];
			else
			{
				Console.Write("Username? ");
				username = Console.ReadLine();
			}

			string password;
			if (args.Length > 2)
				password = args[2];
			else
			{
				Console.Write("Password? ");
				password = ReadPassword();
				if (password == null)
					return null;
			}

			return GetClient(productInformation, username, password);
		}

		private static bool IsAltOrControl(ConsoleKeyInfo keyInfo) =>
			(keyInfo.Modifiers & (ConsoleModifiers.Alt | ConsoleModifiers.Control)) != 0;

		private static string ReadPassword()
		{
			var builder = new StringBuilder();

			while (true)
			{
				ConsoleKeyInfo keyInfo = Console.ReadKey(true);
				if (IsAltOrControl(keyInfo))
					continue;

				switch (keyInfo.Key)
				{
					case ConsoleKey.Backspace:
						if (builder.Length > 0)
							builder.Length--;
						break;

					case ConsoleKey.Enter:
						Console.WriteLine();
						return builder.ToString();

					case ConsoleKey.Escape:
						Console.WriteLine();
						return null;

					default:
						char chr = keyInfo.KeyChar;
						if (chr > 0)
							builder.Append(chr);
						else
							break;
						break;
				}
			}
		}

		private static bool TryGetClient(string[] args, ProductHeaderValue productionInformation, out GitHubClient client)
		{
			if (args.Length > 0 && args[0].Length > 0)
				return TryGetClient(args, args[0][0], productionInformation, out client);

			while (true)
			{
				if (!TryReadAuthenticationKey(out char key))
					continue;

				if (key == (char)ConsoleKey.Escape)
				{
					client = null;
					return false;
				}

				if (TryGetClient(args, key, productionInformation, out client))
					return true;
			}
		}

		private static bool TryGetClient(string[] args, char chr, ProductHeaderValue productionInformation, out GitHubClient client)
		{
			switch (chr)
			{
				case 'b':
				case 'B':
					client = AuthenticateBasic(args, productionInformation);
					return client != null;

				case 't':
				case 'T':
					client = AuthenticateToken(args, productionInformation);
					return client != null;

				case 'u':
				case 'U':
					client = new GitHubClient(productionInformation);
					return client != null;

				default:
					Console.WriteLine($"Invalid authentication type.");
					client = null;
					return false;
			}
		}

		private static bool TryReadAuthenticationKey(out char result)
		{
			Console.Write("Authentication (B=Basic, T=Token, U=Unauthenticated, Esc=Exit)? ");
			ConsoleKeyInfo keyInfo = Console.ReadKey();
			Console.WriteLine();

			if (IsAltOrControl(keyInfo))
			{
				result = (char)0;
				return false;
			}

			result = keyInfo.KeyChar;
			return true;
		}

		private static async Task TestFeature(GitHubClient client)
		{
			Console.WriteLine("Available features:");
			Console.WriteLine("  C=Code");
			Console.WriteLine("  F=Fork");
			Console.WriteLine("  I=Issue");
			Console.WriteLine("  L=Label");
			Console.WriteLine("  R=Repository");
			Console.WriteLine("  U=User");
			Console.WriteLine("  Esc=Exit");

			while (true)
			{
				Console.Write("Feature? ");
				ConsoleKeyInfo keyInfo = Console.ReadKey();
				Console.WriteLine();

				if (IsAltOrControl(keyInfo))
					continue;

				switch (keyInfo.Key)
				{
					case ConsoleKey.C:
						Console.WriteLine("Testing Code...");
						await CodeExamples(client);
						break;

					case ConsoleKey.F:
						Console.WriteLine("Testing Fork...");
						await RepositoryForksExample(client);
						break;

					case ConsoleKey.I:
						Console.WriteLine("Testing Issue...");
						await IssueExamples(client);
						break;

					case ConsoleKey.L:
						Console.WriteLine("Testing Label...");
						await LabelExamples(client);
						break;

					case ConsoleKey.R:
						Console.WriteLine("Testing Repository...");
						await RepositoryExamples(client);
						break;

					case ConsoleKey.U:
						Console.WriteLine("Testing User...");
						await UserExamples(client);
						break;

					case ConsoleKey.Escape:
						return;

					default:
						Console.WriteLine("Invalid selection.");
						break;
				}

			}
		}
	}
}
