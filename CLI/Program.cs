using Entities;
using RepositoryContracts;
using FileRepositories;
using CLI.UI;

IUserRepository userRepository = new UserFileRepository();
IPostRepository postRepository = new PostFileRepository();
ICommentRepository commentRepository = new CommentFileRepository();

CliApp cliApp = new CliApp(userRepository, postRepository, commentRepository);

await cliApp.StartAsync();