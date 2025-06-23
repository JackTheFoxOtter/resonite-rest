using System;
using System.Collections.Generic;

namespace ApiFramework.Resources
{
    /// <summary>
    /// Represents the "path" to an API item inside of a resource.
    /// The path is the collection of keys required to reach that item from the root.
    /// </summary>
    public class ApiPropertyPath : IComparable<ApiPropertyPath>
    {
        private string? _cachedFullPath;

        public static ApiPropertyPath Root = new ApiPropertyPath();
        public string[] PathSegments { get; }
        public string FullPath => GetFullPath();
        public int Length => PathSegments.Length;

        /// <summary>
        /// Creates a new ApiPropertyPath with one segment.
        /// </summary>
        /// <param name="pathSegment">String key of the path segment to append.</param>
        /// <exception cref="ArgumentException">When a path segment key contains '.'.</exception>
        public ApiPropertyPath(string pathSegment) : this(null, new string[] { pathSegment }) { }

        /// <summary>
        /// Creates a new ApiPropertyPath with one or more segments.
        /// </summary>
        /// <param name="segments">String keys of path segments to append.</param>
        /// <exception cref="ArgumentException">When a path segment key contains '.'.</exception>
        public ApiPropertyPath(params string[] segments) : this(null, segments) { }

        /// <summary>
        /// Creates a new ApiPropertyPath extending an existing one appended by one segment.
        /// </summary>
        /// <param name="other">Existing ApiPropertyPath to extend.</param>
        /// <param name="pathSegment">String key of the path segment to append.</param>
        /// <exception cref="ArgumentException">When a path segment key contains '.'.</exception>
        public ApiPropertyPath(ApiPropertyPath? other, string pathSegment) : this(other, new string[] { pathSegment }) { }

        /// <summary>
        /// Creates a new ApiPropertyPath extending an existing one appended by one ore more segments.
        /// </summary>
        /// <param name="other">Existing ApiPropertyPath to extend.</param>
        /// <param name="segments">String keys of path segments to append.</param>
        /// <exception cref="ArgumentException">When a path segment key contains '.'.</exception>
        public ApiPropertyPath(ApiPropertyPath? other, params string[] segments)
        {
            List<string> pathSegmentList = new();
            if (other != null)
            {
                pathSegmentList.AddRange(other.PathSegments);
            }
            foreach (string segment in segments)
            {
                if (segment.Contains(".")) 
                    throw new ArgumentException("Path segment can't contain '.' character. (To create an instance from a full path, use ApiPropertyPath.FromFullPath(string))");
                
                pathSegmentList.Add(segment);
            }
            if (pathSegmentList.Count == 0 || !string.IsNullOrEmpty(pathSegmentList[0]))
            {
                // Ensure implicit root path segment
                pathSegmentList.Insert(0, string.Empty);
            }
            PathSegments = pathSegmentList.ToArray();
        }

        /// <summary>
        /// Returns a new instance with the current instance's path appended by the provided path segment.
        /// </summary>
        /// <param name="pathSegment">String key of the path segment to append.</param>
        /// <returns>New ApiPropertyPath instance.</returns>
        public ApiPropertyPath Append(string pathSegment) => new(this, pathSegment);

        /// <summary>
        /// Returns a new instance with the current instance's path appended by the provided path segment(s).
        /// </summary>
        /// <param name="segments">String keys of path segments to append.</param>
        /// <returns>New ApiPropertyPath instance.</returns>
        public ApiPropertyPath Append(params string[] segments) => new(this, segments);

        /// <summary>
        /// Retrieves a single path segment at index.
        /// </summary>
        /// <param name="index">Index of desired segment.</param>
        /// <returns>Segment at index.</returns>
        public string this[int index]
        {
            get => PathSegments[index];
        }

        /// <summary>
        /// The full path is all path segments joined by '.'.
        /// </summary>
        /// <returns>Full path string.</returns>
        private string GetFullPath()
        {
            if (_cachedFullPath == null)
            {
                _cachedFullPath = string.Join(".", PathSegments);
            }
            return _cachedFullPath;
        }

        /// <summary>
        /// Checks wether two ApiPropertyPath instances are considered equal.
        /// </summary>
        /// <param name="other">Object to compare against</param>
        /// <returns>Whether x & y are equal.</returns>
        public override bool Equals(object other)
        {
            if (other == null) return false;
            if (other is not ApiPropertyPath otherPath) return false;
            if (otherPath.Length != Length) return false; // Different number of segments -> not equal
            for (int i = 0; i < Length; i++)
            {
                if (otherPath.PathSegments[i] != PathSegments[i]) return false; // Different segment -> not equal
            }
            return true; // Equal
        }

        /// <summary>
        /// Hash code of ApiPropertyPath instance.
        /// </summary>
        /// <returns>Hash code of ApiPropertyPath.</returns>
        public override int GetHashCode()
        {
            return FullPath.GetHashCode();
        }

        /// <summary>
        /// Compares own sorting order relative to another ApiPropertyPath.
        /// </summary>
        /// <param name="other">ApiPropertyPath to compare against.</param>
        /// <returns>Sorting order relative to other.</returns>
        public int CompareTo(ApiPropertyPath other)
        {
            if (other.FullPath.Length > FullPath.Length) return -1;
            if (other.FullPath.Length < FullPath.Length) return 1;
            return FullPath.CompareTo(other.FullPath);
        }

        /// <summary>
        /// Creates a new ApiPropertyPath from a full path input.
        /// </summary>
        /// <param name="fullPath">Full path ('.' separated)</param>
        /// <returns>New ApiPropertyPath instance</returns>
        public static ApiPropertyPath FromFullPath(string fullPath)
        {
            return new(fullPath.Split('.'));
        }

        /// <summary>
        /// Overrides default ToString() implementation to return the FullPath property.
        /// </summary>
        /// <returns>FullPath of the instance.</returns>
        public override string ToString()
        {
            return $"<root>{FullPath}";
        }
    }
}
