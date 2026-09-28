namespace HCA.Models.Enums
{
    /// <summary>
    /// Enumeration representing the different types of notifications that can be received from Verato.
    /// </summary>
    public enum VeratoNotificationTypes
    {
        /// <summary>
        /// A postIdentity request is rejected because an overlay was detected. 
        /// New attributes are too different from old attributes.
        /// </summary>
        RejectedOverlay,

        /// <summary>
        /// A new identity with a new native ID was posted for the first time and received an initial Link ID assignment.
        /// </summary>
        IdentityIngested,

        /// <summary>
        /// A native id has a change in assigned Link ID due to a postIdentity update
        /// the postIdentity update included new attribute values that allowed Universal Identity to recognize a better match, resulting in a new Link ID assignment.
        /// </summary>
        MergeIdentities,

        /// <summary>
        /// An overlay associated with a native ID was accepted from the 'Rejected Overlays' task queue. 
        /// </summary>
        AppliedOverlay,

        /// <summary>
        /// A record with dummy patient data was updated with real patient data without triggering any overlay.
        /// </summary>
        DummyDataOverwritten,

        /// <summary>
        /// A record in a postIdentity request gets matched with a Verato for Enrich Identity.
        /// </summary>
        IdentityEnriched,

        /// <summary>
        /// The linkIdentities web service was invoked to two native IDs.
        /// </summary>
        LinkIdentities,

        /// <summary>
        /// The unlinkIdentities web service was invoked to unlink a native ID out of its current Link ID into a brand new Link ID.
        /// </summary>
        UnlinkIdentities,

        /// <summary>
        /// The unlinkIdentities web service was invoked to automatically unlink a native ID, out of its current Link ID into a brand new Link ID.
        /// </summary>
        AutoUnlinkIdentities,

        /// <summary>
        /// The mergeIdentities web service was invoked to merge a native ID into a different Link ID.
        /// </summary>
        SourceRetired,

        /// <summary>
        /// The unmergeIdentities web service was invoked to unmerge a native ID out of its current Link ID into a brand new Link ID.
        /// </summary>
        SourceUnRetired,

        /// <summary>
        /// The deleteSourceIdentity web service was invoked to delete native ID. Note that in this case, 
        /// the “newLinkId” is reported as the same value as “previousLinkId” the Link ID did not truly change, 
        /// rather the source record was deleted out of Universal Identity (while other source records still exist in that Link ID).
        /// </summary>
        SourceDeleted,

        /// <summary>
        /// The deleteSourceIdentity web service was invoked to delete a native ID
        /// </summary>
        HardDeleted,

        /// <summary>
        /// The deactivateSourceWS web service was invoked to deactivate a native ID.
        /// </summary>
        SourceDeactivated,

        /// <summary>
        /// The reactivateSourceWS web service was invoked to reactivate a native ID. 
        /// The record was originally under one Link ID and was assigned a new Link ID.
        /// </summary>
        SourceReactivated,

        /// <summary>
        /// A postIdentity request resulted in the creation of a new Link ID
        /// that was assigned a household.
        /// </summary>
        HouseholdAssigned,

        /// <summary>
        /// A single Link ID 67c0a7125948292d5ab22cd6 in a household with ID 67cb0323cca83013d01e9229
        /// </summary>
        HouseholdDeleted

    }
}
