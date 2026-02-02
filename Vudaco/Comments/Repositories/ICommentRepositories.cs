using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Vudaco.Comments.Dtos;
using Vudaco.Comments.Models;
using Vudaco.Shares;
using Vudaco.Shares.BaseRepository;

namespace Vudaco.Comments.Repositories
{
    public interface ICommentRepositories : IBaseRepository<Comment>
    {
        Task<PaginatedResultReact<object>> GetObjectTaskAsync(CommentDto CommentDto, int page, int pageSize, CancellationToken cancellationToken);
        Task<Comment> ShowAsync(int id);
        Task<Comment> CreateAsync(Comment Comment);
        Task<Comment> UpdateAsync(Comment Comment);
        Task<Comment> DeleteSoftAsync(Comment Comment);
    }
}
