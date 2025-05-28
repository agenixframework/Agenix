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

namespace Agenix.Screenplay;

/// <summary>
///     A performable task that can be created anonymously with a title and a list of steps.
/// </summary>
public class AnonymousPerformable : IPerformable, IHasCustomFieldValues
{
    private readonly Dictionary<string, object> _fieldValues = new();
    private readonly List<IPerformable> _steps;
    private readonly string _title;

    public AnonymousPerformable()
    {
    }

    public AnonymousPerformable(string title, List<IPerformable> steps)
    {
        _title = title;
        _steps = steps;
    }

    public IDictionary<string, object> CustomFieldValues =>
        new Dictionary<string, object>(_fieldValues);

    public void PerformAs<T>(T actor) where T : Actor
    {
        actor.AttemptsTo(_steps.ToArray());
    }

    public void SetFieldValue(string fieldName, object fieldValue)
    {
        _fieldValues[fieldName] = fieldValue;
    }

    public override string ToString()
    {
        return _title;
    }
}
