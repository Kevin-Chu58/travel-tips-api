using System.Drawing;
using TravelTipsAPI.Constants;
using TravelTipsAPI.Models.TravelTipsModels;
using TravelTipsAPI.ViewModels.db_basic;
using TravelTipsAPI.ViewModels.db_gospel;
using TravelTipsAPI.ViewModels.db_sermon;
using static TravelTipsAPI.Services.TravelTipsServices.BasicSchema;
using static TravelTipsAPI.Services.TravelTipsServices.GospelSchema;
using static TravelTipsAPI.Utils.StringUtils;

namespace TravelTipsAPI.Services.TravelTipsServices
{
    public class SermonsService(TravelTipsContext context, IUsersService usersService)
        : ISermonsService
    {
        // sermons

        /// <summary>
        /// Get sermon by id
        /// </summary>
        /// <param name="id">id</param>
        /// <param name="allowNull">allow null</param>
        /// <param name="isRestricted">whether user can see future sermons</param>
        /// <returns>the sermon with the id</returns>
        public Sermon? GetSermonById(int id, bool allowNull = false, bool isRestricted = false)
        {
            var sermon = context.Sermons.Find(id);
            if (sermon is null && !allowNull)
                throw new Exception(Messages.SermonNotFound);

            var today = DateOnly.FromDateTime(DateTime.Now);
            if (!isRestricted && (sermon?.PublishAt > today || sermon?.LabelId is null))
                throw new Exception(Messages.SermonUnauthorized);

            return sermon;
        }

        /// <summary>
        /// Get a sermon by label and order
        /// </summary>
        /// <param name="label">sermon label</param>
        /// <param name="order">nth in order</param>
        /// <returns>the sermon</returns>
        public Sermon? GetSermonByLabelOrder(SermonLabel label, int order)
        {
            var sermon = context
                .Sermons.Where(s => s.LabelId == label.Id)
                .OrderBy(s => s.PublishAt)
                .Skip(order - 1)
                .FirstOrDefault();

            return sermon;
        }

        /// <summary>
        /// Get the sermon order based on sermon
        /// </summary>
        /// <param name="sermon">sermon</param>
        /// <returns>the sermon order</returns>
        public int GetSermonOrder(Sermon sermon)
        {
            // Count how many sermons have smaller Id in the same label
            var order = context
                .Sermons.Where(s => s.LabelId == sermon.LabelId && s.PublishAt <= sermon.PublishAt)
                .OrderBy(s => s.PublishAt) // ascending by publish date
                .Count();

            return order;
        }

        /// <summary>
        /// Get a list of latest sermons in banner
        /// </summary>
        /// <returns>a list of latest sermons</returns>
        public IEnumerable<SermonViewModel> GetLatestSermons()
        {
            var today = DateOnly.FromDateTime(DateTime.Now);

            var sermons = context
                .Sermons.Where(s =>
                    s.IsBanner
                    && s.PublishAt
                        == context
                            .Sermons.Where(s => s.PublishAt <= today && s.LabelId != null)
                            .Max(s => s.PublishAt)
                )
                .ToList();

            return sermons.Select(s => GetSermonViewModel(s));
        }

        /// <summary>
        /// Get a list of sermons by params
        /// </summary>
        /// <param name="createdBy">writer user id</param>
        /// <param name="title">title</param>
        /// <param name="label">label</param>
        /// <param name="isBanner">do sermons appear in banner</param>
        /// <param name="isRestricted">whether user can see future sermons</param>
        /// <param name="isDesc">whether is in descending or ascending order</param>
        /// <returns></returns>
        public IEnumerable<SermonViewModel> GetSermonsByParams(
            int? createdBy = null,
            string? title = null,
            SermonLabel? label = null,
            bool? isBanner = null,
            bool isRestricted = false,
            bool isDesc = true
        )
        {
            var query = context.Sermons.AsQueryable();

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
            if (isBanner != null)
            {
                query = query.Where(s => s.IsBanner == isBanner);
            }
            if (!isRestricted)
            {
                var today = DateOnly.FromDateTime(DateTime.Now);
                query = query.Where(s => s.PublishAt <= today).Where(s => s.LabelId != null);
            }

            query = isDesc
                ? query.OrderByDescending(s => s.PublishAt)
                : query.OrderBy(s => s.PublishAt);

            var sermons = query.ToList();

            var results = sermons.Select(s => GetSermonViewModel(s)).ToList();
            return results;
        }

        /// <summary>
        /// Get the sermon view model from sermon, optionally add content
        /// </summary>
        /// <param name="sermon">sermon</param>
        /// <param name="hasContent">includes content in the view model</param>
        /// <returns>the sermon view model</returns>
        public SermonViewModel GetSermonViewModel(Sermon sermon, bool hasContent = false)
        {
            var sermonViewModel = new SermonViewModel
            {
                Id = sermon.Id,
                CreatedBy = (UserSimpleViewModel)usersService.GetUserById(sermon.CreatedBy),
                Title = sermon.Title,
                Content = hasContent ? sermon.Content : null,
                Label = BuildSermonLabelComplete(sermon.LabelId),
                PublishAt = sermon.PublishAt,
            };

            return sermonViewModel;
        }

        /// <summary>
        /// Get a list of sermon ids by user id
        /// </summary>
        /// <param name="userId">user id</param>
        /// <returns>a list of sermon ids owned by the user</returns>
        public IEnumerable<int> GetMySermons(int userId)
        {
            return context.Sermons.Where(s => s.CreatedBy == userId).Select(l => l.Id).ToList();
        }

        /// <summary>
        /// Create a new sermon
        /// </summary>
        /// <param name="sermonPost">new sermon</param>
        /// <returns>the new sermon</returns>
        public async Task<SermonViewModel> PostSermon(SermonPostViewModel sermonPost)
        {
            var newSermon = new Sermon
            {
                Title = sermonPost.Title,
                Content = sermonPost.Content,
                LabelId = sermonPost.LabelId,
                PublishAt = sermonPost.PublishAt,
                IsBanner = sermonPost.IsBanner ?? false,
            };

            await context.Sermons.AddAsync(newSermon);
            await context.SaveChangesAsync();

            return GetSermonViewModel(newSermon);
        }

        /// <summary>
        /// Update an existing sermon
        /// </summary>
        /// <param name="sermon">sermon to be updated</param>
        /// <param name="sermonPatch">updated sermon details</param>
        /// <returns>the updated sermon</returns>
        public async Task<SermonViewModel> PatchSermon(
            Sermon sermon,
            SermonPatchViewModel sermonPatch
        )
        {
            sermon.Title = sermonPatch.Title ?? sermon.Title;
            sermon.Content = sermonPatch.Content ?? sermon.Content;
            sermon.LabelId = sermonPatch.LabelId ?? sermon.LabelId;
            sermon.PublishAt = sermonPatch.PublishAt ?? sermon.PublishAt;
            sermon.IsBanner = sermonPatch.IsBanner ?? sermon.IsBanner;

            await context.SaveChangesAsync();

            return GetSermonViewModel(sermon);
        }

        /// <summary>
        /// Delete a sermon
        /// </summary>
        /// <param name="sermon">sermon to be deleted</param>
        /// <returns>the deleted sermon id</returns>
        public async Task<int> DeleteSermon(Sermon sermon)
        {
            var sermonId = sermon.Id;

            context.Remove(sermon);
            await context.SaveChangesAsync();

            return sermonId;
        }

        // sermon labels

        /// <summary>
        /// Get sermon label by id
        /// </summary>
        /// <param name="id">id</param>
        /// <param name="allowNull">allow null</param>
        /// <returns>sermon label</returns>
        public SermonLabel? GetLabelById(int id, bool allowNull = false)
        {
            var sermonLabel = context.SermonLabels.Find(id);
            if (sermonLabel is null && !allowNull)
                throw new Exception(Messages.SermonLabelNotFound);

            return sermonLabel;
        }

        /// <summary>
        /// Get sermon label by slug
        /// </summary>
        /// <param name="slug">slug</param>
        /// <returns>sermon label with that slug</returns>
        public SermonLabel? GetLabelBySlug(string slug)
        {
            var sermonLabel = context.SermonLabels.FirstOrDefault(l => l.Slug == slug);

            return sermonLabel;
        }

        /// <summary>
        /// Get a list of sermon labels by params
        /// </summary>
        /// <param name="name">label name</param>
        /// <param name="parentLabelId">parent label id</param>
        /// <param name="type">label type</param>
        /// <returns>a list of sermons that fit the params</returns>
        public IEnumerable<SermonLabelViewModel> GetLabelsByParams(
            string? name = null,
            int? parentLabelId = null,
            string? type = null
        )
        {
            var query = context.SermonLabels.AsQueryable();

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

            var result = labels.Select(l => (SermonLabelViewModel)l);
            return result;
        }

        /// <summary>
        /// Generate the complete sermon label by label id
        /// </summary>
        /// <param name="id">id</param>
        /// <returns>a complete sermon label</returns>
        public SermonLabelCompleteViewModel? BuildSermonLabelComplete(int? id)
        {
            if (id is null)
                return null;

            var label = GetLabelById((int)id);

            SermonLabelCompleteViewModel labelComplete = new();

            while (label != null)
            {
                switch (label.Type)
                {
                    case "Category":
                        labelComplete.Category = (SermonLabelViewModel)label;
                        break;
                    case "Topic":
                        labelComplete.Topic = (SermonLabelViewModel)label;
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
        /// Create a new sermon label
        /// </summary>
        /// <param name="name">sermon label name</param>
        /// <param name="type">sermon label type</param>
        /// <returns>the created sermon label</returns>
        public async Task<SermonLabelViewModel> PostNewLabel(string name, string type)
        {
            var slug = StrToSlug(name);

            if (DoesNameExist(slug))
                throw new Exception(Messages.SermonLabelExists);

            IEnumerable<string> validTypes = ["Category", "Topic"];
            if (validTypes.All(t => t != type))
                throw new Exception(Messages.SermonLabelTypeInvalid);

            var newLabel = new SermonLabel
            {
                Name = name,
                Slug = slug,
                Type = type,
            };

            await context.AddAsync(newLabel);
            await context.SaveChangesAsync();

            return (SermonLabelViewModel)newLabel;
        }

        /// <summary>
        /// Update an existing sermon label name
        /// </summary>
        /// <param name="label">existing label</param>
        /// <param name="newName">the new label name to be updated</param>
        /// <returns>the updated sermon label</returns>
        public async Task<SermonLabelViewModel> UpdateLabel(SermonLabel label, string newName)
        {
            var newSlug = StrToSlug(newName);

            if (DoesNameExist(newSlug))
                throw new Exception(Messages.SermonLabelExists);

            label.Name = newName;
            label.Slug = newSlug;

            await context.SaveChangesAsync();
            return (SermonLabelViewModel)label;
        }

        /// <summary>
        /// Delete an existing sermon label
        /// </summary>
        /// <param name="label">label to be deleted</param>
        /// <returns>the deleted sermon label id</returns>
        public async Task<int> DeleteLabel(SermonLabel label)
        {
            var id = label.Id;
            IEnumerable<SermonLabel> labels = [label];

            // delete children labels if exist
            var children = context.SermonLabels.Where(l => l.ParentLabelId == id).ToList();
            labels = labels.Concat(children);

            // remove all sermons with the labels
            var labelIds = labels.Select(l => l.Id).ToList();

            var sermons = context.Sermons.Where(s => labelIds.Contains(s.LabelId ?? 0)).ToList();

            foreach (Sermon s in sermons)
            {
                s.LabelId = null;
            }

            context.RemoveRange(labels);
            await context.SaveChangesAsync();

            return id;
        }

        public bool DoesNameExist(string slug)
        {
            return context.SermonLabels.Any(l => l.Slug == slug);
        }
    }
}
