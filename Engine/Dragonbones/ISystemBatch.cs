using System;
using System.Collections.Generic;
using System.Text;

namespace Dragonbones
{
    public interface ISystemBatch
    {
        /// <summary>
        /// How many systems are in this batch
        /// </summary>
        int Count { get; }
        /// <summary>
        /// Gets the estimated length of the batch
        /// </summary>
        double Length { get; }
        /// <summary>
        /// Is this batch finished running
        /// </summary>
        bool Finished { get; }
        /// <summary>
        /// Is a lane finished running
        /// </summary>
        /// <param name="laneID">the id of the lane to check</param>
        /// <returns>if the specified lane is finished</returns>
        bool IsLaneFinished(int laneID);
        /// <summary>
        /// Add a system to the given lane
        /// </summary>
        /// <param name="laneID">the id of the lane</param>
        /// <param name="sysInf">the system info to add</param>
        /// <param name="avgLength">the average time for the system to finish</param>
        void AppendToLane(int laneID, SystemInfo sysInf, double avgLength);
        /// <summary>
        /// Appends lane2 onto the end of lane1 (clearing out lane2)
        /// </summary>
        /// <param name="lane1ID">the id of the first lane</param>
        /// <param name="lane2ID">the id of the second lane</param>
        void MergeLanes(int lane1ID, int lane2ID);
        /// <summary>
        /// Swaps the two lanes
        /// </summary>
        /// <param name="lane1ID">the id of the first lane</param>
        /// <param name="lane2ID">the id of the second lane</param>
        void SwapLanes(int lane1ID, int lane2ID);
        /// <summary>
        /// How long it is expected to take to run this lane
        /// </summary>
        /// <param name="laneID"></param>
        /// <returns></returns>
        double LaneLenth(int laneID);
        /// <summary>
        /// Get the next system in the given lane
        /// </summary>
        /// <param name="laneID">the id of the lane to retrieve from</param>
        /// <param name="sysInf">the system info retrieved</param>
        /// <returns>If there is another system in the lane</returns>
        bool NextSystem(int laneID, out SystemInfo sysInf);
        /// <summary>
        /// Reset a given lane to the first system
        /// </summary>
        /// <param name="laneID">the id of the lane to reset</param>
        void ResetLane(int laneID);
        /// <summary>
        /// Reset all lanes to the first system
        /// </summary>
        void Reset();
        /// <summary>
        /// Clear the data from the batch
        /// </summary>
        void Clear();
    }
}
