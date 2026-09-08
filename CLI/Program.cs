using Entities;
using RepositoryContracts;
using InMemoryRepositories;
using CLI.UI;

IUserRepository userRepository = new UserInMemoryRepository();
IPostRepository postRepository = new PostInMemoryRepository();
ICommentRepository commentRepository = new CommentInMemoryRepository();

CliApp cliApp = new CliApp(userRepository, postRepository, commentRepository);

await cliApp.StartAsync();