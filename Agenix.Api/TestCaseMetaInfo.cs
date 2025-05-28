#region License

// MIT License
//
// Copyright (c) 2025 Agenix
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

#endregion

namespace Agenix.Core;

/// Test case meta-information.
/// /
public class TestCaseMetaInfo
{
    /// Represents the status of a test case.
    public enum Status
    {
        DRAFT,
        READY_FOR_REVIEW,
        FINAL,
        DISABLED
    }

    /// Stores the initial creation date of the test case.
    /// /
    private DateTime _creationDate;

    /// Stores the last updated date and time of the test case.
    private DateTime _lastUpdatedOn;

    public Status _status = Status.DRAFT;

    /// Represents the individual or entity responsible for the creation or ownership
    /// of the test case.
    public string Author { get; set; }

    /// Stores information about the user who last updated the test case.
    public string LastUpdatedBy { get; set; }

    /// Represents the creation date of the test case.
    /// This indicates when the test case was initially defined or created.
    public DateTime CreationDate
    {
        get => new(_creationDate.Ticks);
        set => _creationDate = new DateTime(value.Ticks);
    }

    /// Stores the last updated date and time of the test case.
    public DateTime LastUpdatedOn
    {
        get => new(_lastUpdatedOn.Ticks);
        set => _lastUpdatedOn = new DateTime(value.Ticks);
    }

    /// Sets the status of the test case.
    /// <param name="newStatus">The new status to be assigned to the test case.</param>
    /// /
    public void SetStatus(Status newStatus)
    {
        _status = newStatus;
    }

    /// Retrieves the current status of the test case.
    /// <return>The current status assigned to the test case.</return>
    public Status GetStatus()
    {
        return _status;
    }
}
