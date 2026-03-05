using TravelTipsAPI.Constants;
using TravelTipsAPI.Models.TravelTipsModels;
using TravelTipsAPI.ViewModels.db_basic;
using TravelTipsAPI.ViewModels.db_gospel;
using static TravelTipsAPI.Services.TravelTipsServices.BasicSchema;
using static TravelTipsAPI.Services.TravelTipsServices.GospelSchema;
using static TravelTipsAPI.Utils.StringUtils;

namespace TravelTipsAPI.Services.TravelTipsServices.Gospel
{
    public class WritingsService(TravelTipsContext context, IUsersService usersService)
        : IWritingsService
    {
        // writings

        /// <summary>
        /// Get writing by id
        /// </summary>
        /// <param name="id">id</param>
        /// <param name="allowNull">allow null</param>
        /// <param name="isRestricted">whether user can see future writings</param>
        /// <returns>the writing with the id</returns>
        public Writing? GetWritingById(int id, bool allowNull = false, bool isRestricted = false)
        {
            var writing = context.Writings.Find(id);
            if (writing is null && !allowNull)
                throw new Exception(Messages.WritingNotFound);

            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            if (!isRestricted && (writing?.PublishAt > today || writing?.LabelId is null))
                throw new Exception(Messages.WritingUnauthorized);

            return writing;
        }

        /// <summary>
        /// Get a writing by label and order
        /// </summary>
        /// <param name="label">writing label</param>
        /// <param name="order">nth in order</param>
        /// <returns>the writing</returns>
        public Writing? GetWritingByLabelOrder(WritingLabel label, int order)
        {
            var writing = context
                .Writings.Where(s => s.LabelId == label.Id)
                .OrderBy(s => s.PublishAt)
                .Skip(order - 1)
                .FirstOrDefault();

            return writing;
        }

        /// <summary>
        /// Get the writing order based on writing
        /// </summary>
        /// <param name="writing">writing</param>
        /// <returns>the writing order</returns>
        public int GetWritingOrder(Writing writing)
        {
            // Count how many writings have smaller Id in the same label
            var order = context
                .Writings.Where(s =>
                    s.LabelId == writing.LabelId && s.PublishAt <= writing.PublishAt
                )
                .OrderBy(s => s.PublishAt) // ascending by publish date
                .Count();

            return order;
        }

        /// <summary>
        /// Get a list of latest writings
        /// </summary>
        /// <returns>a list of latest writings</returns>
        //public async Task<IEnumerable<WritingViewModel>> GetLatestWritings()
        //{
        //    var today = DateOnly.FromDateTime(DateTime.Now);

        //    var writings = context
        //        .Writings.Where(s =>
        //            s.IsBanner
        //            && s.PublishAt
        //                == context
        //                    .Writings.Where(s => s.PublishAt <= today && s.LabelId != null)
        //                    .Max(s => s.PublishAt)
        //        )
        //        .ToList();

        //    var results = new List<WritingViewModel>();

        //    foreach (var writing in writings)
        //    {
        //        results.Add(await GetWritingViewModel(writing));
        //    }

        //    return results;
        //}

        /// <summary>
        /// Get a list of writings by params
        /// </summary>
        /// <param name="createdBy">writer user id</param>
        /// <param name="title">title</param>
        /// <param name="label">label</param>
        /// <param name="isRestricted">whether user can see future writings</param>
        /// <param name="isDesc">whether is in descending or ascending order</param>
        /// <returns></returns>
        public async Task<IEnumerable<WritingViewModel>> GetWritingsByParams(
            int? createdBy = null,
            string? title = null,
            WritingLabel? label = null,
            bool isRestricted = false,
            bool isDesc = true
        )
        {
            var query = context.Writings.AsQueryable();

            if (createdBy != null)
            {
                query = query.Where(s => s.CreatedBy == createdBy);
            }
            if (title != null)
            {
                query = query.Where(s => s.Title.Contains(title));
            }
            if (label != null && label.Type == "Topic")
            {
                query = query.Where(s => s.LabelId == label.Id);
            }
            if (!isRestricted)
            {
                var today = DateOnly.FromDateTime(DateTime.UtcNow);
                query = query.Where(s => s.PublishAt <= today).Where(s => s.LabelId != null);
            }

            query = isDesc
                ? query.OrderByDescending(s => s.PublishAt)
                : query.OrderBy(s => s.PublishAt);

            var writings = query.ToList();

            var results = new List<WritingViewModel>();

            foreach (var writing in writings)
            {
                results.Add(await GetWritingViewModel(writing));
            }

            return results;
        }

        /// <summary>
        /// Get the writing view model from writing, optionally add content
        /// </summary>
        /// <param name="writing">writing</param>
        /// <param name="hasContent">includes content in the view model</param>
        /// <returns>the writing view model</returns>
        public async Task<WritingViewModel> GetWritingViewModel(
            Writing writing,
            bool hasContent = false
        )
        {
            var user = usersService.GetUserById(writing.CreatedBy);
            var simpleUser = hasContent
                ? (await usersService.GetUserSimpleViewModels([user])).First()
                : (UserSimpleViewModel)user;

            var writingViewModel = new WritingViewModel
            {
                Id = writing.Id,
                CreatedBy = simpleUser,
                Title = writing.Title,
                Content = hasContent ? writing.Content : null,
                Label = BuildWritingLabelComplete(writing.LabelId),
                PublishAt = writing.PublishAt,
            };

            return writingViewModel;
        }

        /// <summary>
        /// Get a list of writing ids by user id
        /// </summary>
        /// <param name="userId">user id</param>
        /// <returns>a list of writing ids owned by the user</returns>
        public IEnumerable<int> GetMyWritings(int userId)
        {
            return context.Writings.Where(s => s.CreatedBy == userId).Select(l => l.Id).ToList();
        }

        /// <summary>
        /// Create a new writing
        /// </summary>
        /// <param name="writingPost">new writing</param>
        /// <returns>the new writing</returns>
        public async Task<WritingViewModel> PostWriting(
            WritingPostViewModel writingPost,
            int createdBy
        )
        {
            var newWriting = new Writing
            {
                Title = writingPost.Title,
                Content = writingPost.Content,
                LabelId = writingPost.LabelId,
                PublishAt = writingPost.PublishAt,
                CreatedBy = createdBy,
            };

            await context.Writings.AddAsync(newWriting);
            await context.SaveChangesAsync();

            return await GetWritingViewModel(newWriting);
        }

        /// <summary>
        /// Update an existing writing
        /// </summary>
        /// <param name="writing">writing to be updated</param>
        /// <param name="writingPatch">updated writing details</param>
        /// <returns>the updated writing</returns>
        public async Task<WritingViewModel> PatchWriting(
            Writing writing,
            WritingPatchViewModel writingPatch
        )
        {
            writing.Title = writingPatch.Title ?? writing.Title;
            writing.Content = writingPatch.Content ?? writing.Content;
            writing.LabelId = writingPatch.LabelId ?? writing.LabelId;
            writing.PublishAt = writingPatch.PublishAt ?? writing.PublishAt;

            await context.SaveChangesAsync();

            return await GetWritingViewModel(writing);
        }

        /// <summary>
        /// Delete a writing
        /// </summary>
        /// <param name="writing">writing to be deleted</param>
        /// <returns>the deleted writing id</returns>
        public async Task<int> DeleteWriting(Writing writing)
        {
            var writingId = writing.Id;

            context.Remove(writing);
            await context.SaveChangesAsync();

            return writingId;
        }

        // writing labels

        /// <summary>
        /// Get writing label by id
        /// </summary>
        /// <param name="id">id</param>
        /// <param name="allowNull">allow null</param>
        /// <returns>writing label</returns>
        public WritingLabel? GetLabelById(int id, bool allowNull = false)
        {
            var writingLabel = context.WritingLabels.Find(id);
            if (writingLabel is null && !allowNull)
                throw new Exception(Messages.WritingLabelNotFound);

            return writingLabel;
        }

        /// <summary>
        /// Get writing label by slug
        /// </summary>
        /// <param name="slug">slug</param>
        /// <returns>writing label with that slug</returns>
        public WritingLabel? GetLabelBySlug(string slug)
        {
            var writingLabel = context.WritingLabels.FirstOrDefault(l => l.Slug == slug);

            return writingLabel;
        }

        /// <summary>
        /// Get a list of writing labels by params
        /// </summary>
        /// <param name="name">label name</param>
        /// <param name="parentLabelId">parent label id</param>
        /// <param name="type">label type</param>
        /// <returns>a list of writings that fit the params</returns>
        public IEnumerable<WritingLabelViewModel> GetLabelsByParams(
            string? name = null,
            int? parentLabelId = null,
            string? type = null
        )
        {
            var query = context.WritingLabels.AsQueryable();

            if (name != null)
            {
                query = query.Where(l => l.Name.Contains(name));
            }
            if (parentLabelId != null)
            {
                query = query.Where(l => l.ParentLabelId == parentLabelId);
            }
            if (type != null)
            {
                query = query.Where(l => l.Type == type);
            }

            var labels = query.ToList();

            var result = labels.Select(l => (WritingLabelViewModel)l);
            return result;
        }

        /// <summary>
        /// Generate the complete writing label by label id
        /// </summary>
        /// <param name="id">id</param>
        /// <returns>a complete writing label</returns>
        public WritingLabelCompleteViewModel? BuildWritingLabelComplete(int? id)
        {
            if (id is null)
                return null;

            var label = GetLabelById((int)id);

            WritingLabelCompleteViewModel labelComplete = new();

            while (label != null)
            {
                switch (label.Type)
                {
                    case "Category":
                        labelComplete.Category = (WritingLabelViewModel)label;
                        break;
                    case "Topic":
                        labelComplete.Topic = (WritingLabelViewModel)label;
                        break;
                }

                if (label.ParentLabelId.HasValue)
                {
                    label = GetLabelById((int)label.ParentLabelId);
                }
                else
                {
                    break;
                }
            }

            return labelComplete;
        }

        /// <summary>
        /// Create a new writing label
        /// </summary>
        /// <param name="name">writing label name</param>
        /// <param name="type">writing label type</param>
        /// <param name="parentLabelId">parent label id</param>
        /// <returns>the created writing label</returns>
        public async Task<WritingLabelViewModel> PostNewLabel(
            string name,
            string type,
            int? parentLabelId = null
        )
        {
            var slug = StrToSlug(name);

            if (DoesNameExist(slug))
                throw new Exception(Messages.WritingLabelExists);

            IEnumerable<string> validTypes = ["Category", "Topic"];
            if (validTypes.All(t => t != type))
                throw new Exception(Messages.WritingLabelTypeInvalid);

            var newLabel = new WritingLabel
            {
                Name = name,
                Slug = slug,
                Type = type,
                ParentLabelId = parentLabelId,
            };

            await context.AddAsync(newLabel);
            await context.SaveChangesAsync();

            return (WritingLabelViewModel)newLabel;
        }

        /// <summary>
        /// Update an existing writing label name
        /// </summary>
        /// <param name="label">existing label</param>
        /// <param name="newName">the new label name to be updated</param>
        /// <returns>the updated writing label</returns>
        /// <param name="parentLabelId">parent label id</param>
        public async Task<WritingLabelViewModel> UpdateLabel(
            WritingLabel label,
            string newName,
            int? parentLabelId = null
        )
        {
            var newSlug = StrToSlug(newName);

            if (DoesNameExist(newSlug))
                throw new Exception(Messages.WritingLabelExists);

            label.Name = newName;
            label.Slug = newSlug;
            label.ParentLabelId = parentLabelId ?? label.ParentLabelId;

            await context.SaveChangesAsync();
            return (WritingLabelViewModel)label;
        }

        /// <summary>
        /// Delete an existing writing label
        /// </summary>
        /// <param name="label">label to be deleted</param>
        /// <returns>the deleted writing label id</returns>
        public async Task<int> DeleteLabel(WritingLabel label)
        {
            var id = label.Id;
            IEnumerable<WritingLabel> labels = [label];

            // delete children labels if exist
            var children = context.WritingLabels.Where(l => l.ParentLabelId == id).ToList();
            labels = labels.Concat(children);

            // remove all writings with the labels
            var labelIds = labels.Select(l => l.Id).ToList();

            var writings = context.Writings.Where(s => labelIds.Contains(s.LabelId ?? 0)).ToList();

            foreach (Writing s in writings)
            {
                s.LabelId = null;
            }

            context.RemoveRange(labels);
            await context.SaveChangesAsync();

            return id;
        }

        public bool DoesNameExist(string slug)
        {
            return context.WritingLabels.Any(l => l.Slug == slug);
        }
    }
}
