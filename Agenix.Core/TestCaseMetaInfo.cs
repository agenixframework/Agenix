using System;

namespace Agenix.Core;

/// Test case meta information.
/// /
public class TestCaseMetaInfo
{
    /**
     * Status of testcase
     */
    public enum Status
    {
        DRAFT,
        READY_FOR_REVIEW,
        FINAL,
        DISABLED
    }

    /**
     * Creation date of testcase
     */
    private DateTime _creationDate;

    /**
     * Last updated on
     */
    private DateTime _lastUpdatedOn;

    public Status status = Status.DRAFT;

    /**
     * Author of testcase
     */
    public string Author { get; set; }

    /**
     * Last updated by
     */
    public string LastUpdatedBy { get; set; }

    /**
     * Get the creation date.
     */
    public DateTime CreationDate
    {
        get => new(_creationDate.Ticks);
        set => _creationDate = new DateTime(value.Ticks);
    }

    /**
     * Get last updating date.
     */
    public DateTime LastUpdatedOn
    {
        get => new(_lastUpdatedOn.Ticks);
        set => _lastUpdatedOn = new DateTime(value.Ticks);
    }

    /**
     * Set the status the this test case.
     */
    public void SetStatus(Status status)
    {
        this.status = status;
    }

    /**
     * Get the status of this test case.
     */
    public Status GetStatus()
    {
        return status;
    }
}