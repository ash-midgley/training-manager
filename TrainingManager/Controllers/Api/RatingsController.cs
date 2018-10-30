﻿using TrainingManager.Dtos;
using TrainingManager.Models;
using Microsoft.AspNet.Identity;
using System.Web.Http;
using System;

namespace TrainingManager.Controllers.Api
{
    public class RatingsController : ApiController
    {
        private readonly IUnitOfWork _unitOfWork;

        public RatingsController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        [HttpPost]
        public IHttpActionResult Rate(RatingDto dto)
        {
            var userId = User.Identity.GetUserId();

            //rating exists
            if (_unitOfWork.Ratings.UserRatingExists(userId, dto.PlanId))
            {
                //PUT - Update value
                var currentRating = _unitOfWork.Ratings.GetUserRating(userId, dto.PlanId);
                currentRating.Value = dto.Value;
                _unitOfWork.Complete();

                _unitOfWork.Plans.UpdateRating(dto.PlanId, _unitOfWork.Ratings.GetRatingAverage(dto.PlanId));
                _unitOfWork.Complete();

                return Ok();
            }

            var rater = User.Identity.GetUserName();
            var rating = new Rating
            {
                Plan = _unitOfWork.Plans.GetUserPlan(dto.PlanId, userId),
                Value = dto.Value,
                PlanId = dto.PlanId,
                RaterId = userId
            };

            _unitOfWork.Ratings.Add(rating);
            _unitOfWork.Complete();
            _unitOfWork.Plans.AddNewRating(dto.PlanId, _unitOfWork.Ratings.GetRatingAverage(dto.PlanId));
            _unitOfWork.Complete();

            return Ok();
        }

        [Route("api/ratings/{planId}")]
        [HttpDelete]
        public IHttpActionResult DeleteRating(int planId)
        {
            var userId = User.Identity.GetUserId();
            var rating = _unitOfWork.Ratings.GetUserRating(userId, planId);

            if (rating == null)
            {
                return NotFound();
            }

            _unitOfWork.Ratings.Remove(rating);
            _unitOfWork.Complete();

            var currentRating = _unitOfWork.Ratings.GetRatingAverage(planId);
            _unitOfWork.Plans.DeleteRating(planId, currentRating);
            _unitOfWork.Complete();

            return Ok(planId);
        }
    }
}
