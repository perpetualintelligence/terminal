﻿/*
    Copyright (c) 2019-2022. All Rights Reserved. Perpetual Intelligence L.L.C.
    https://perpetualintelligence.com
    https://api.perpetualintelligence.com
*/

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace PerpetualIntelligence.Cli.Commands
{
    /// <summary>
    /// A context-specific command argument.
    /// </summary>
    /// <remarks>
    /// A argument implements the default equality <see cref="IEquatable{T}"/> and <see cref="GetHashCode()"/> using
    /// <see cref="Id"/> and <see cref="Name"/>. Thus, two arguments with the same id and name are equal irrespective of
    /// other property values.
    /// </remarks>
    public sealed class Argument : IEquatable<Argument?>
    {
        /// <summary>
        /// Initialize a new instance..
        /// </summary>
        /// <param name="argumentIdentity">The argument identity.</param>
        /// <param name="value">The argument value.</param>
        public Argument(ArgumentIdentity argumentIdentity, object value)
        {
            Id = argumentIdentity.Id;
            Name = argumentIdentity.Name;
            DataType = argumentIdentity.DataType;
            CustomDataType = argumentIdentity.CustomDataType;
            Description = argumentIdentity.Description;
            Value = value;
        }

        /// <summary>
        /// Initialize a new instance..
        /// </summary>
        /// <param name="id">The argument id.</param>
        /// <param name="name">The argument name.</param>
        /// <param name="value">The argument value.</param>
        public Argument(string id, string name, object value)
        {
            Id = id;
            Name = name;
            Value = value;
        }

        /// <summary>
        /// Initialize a new instance..
        /// </summary>
        /// <param name="id">The argument id.</param>
        /// <param name="name">The argument name.</param>
        /// <param name="value">The argument value.</param>
        /// <param name="customDataType">The argument custom data type.</param>
        public Argument(string id, string name, object value, string customDataType)
        {
            Id = id;
            Name = name;
            Value = value;
            DataType = DataType.Custom;
            CustomDataType = customDataType;
        }

        /// <summary>
        /// The argument custom data type.
        /// </summary>
        /// <remarks>This custom data type is used only if the <see cref="DataType"/> property is set to <see cref="DataType.Custom"/>.</remarks>
        [JsonPropertyName("custom_data_type")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? CustomDataType { get; set; }

        /// <summary>
        /// The argument data type. Defaults to <see cref="DataType.Text"/>.
        /// </summary>
        [JsonPropertyName("data_type")]
        public DataType DataType { get; set; } = DataType.Text;

        /// <summary>
        /// The argument description.
        /// </summary>
        [JsonPropertyName("description")]
        public string? Description { get; set; }

        /// <summary>
        /// The argument identifier.
        /// </summary>
        /// <remarks>The argument identifier is unique across all commands.</remarks>
        [JsonPropertyName("id")]
        public string Id { get; set; }

        /// <summary>
        /// The argument name.
        /// </summary>
        /// <remarks>The argument name is unique with in a command.</remarks>
        [JsonPropertyName("name")]
        public string Name { get; set; }

        /// <summary>
        /// The argument value.
        /// </summary>
        /// <remarks>The argument name is unique with in a command.</remarks>
        [JsonPropertyName("value")]
        public object Value { get; set; }

        /// <inheritdoc/>
        public static bool operator !=(Argument? left, Argument? right)
        {
            return !(left == right);
        }

        /// <inheritdoc/>
        public static bool operator ==(Argument? left, Argument? right)
        {
            return EqualityComparer<Argument>.Default.Equals(left, right);
        }

        /// <inheritdoc/>
        public override bool Equals(object? obj)
        {
            return Equals(obj as Argument);
        }

        /// <inheritdoc/>
        public bool Equals(Argument? other)
        {
            return other != null &&
                   Id == other.Id &&
                   Name == other.Name;
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return HashCode.Combine(Id, Name);
        }
    }
}
