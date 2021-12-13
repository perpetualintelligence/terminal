/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved
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
    /// A <c>cli</c> command argument.
    /// </summary>
    /// <remarks>
    /// <para>
    /// An argument id is always unique within a command. By design <see cref="Argument"/> implements the default
    /// equality <see cref="IEquatable{T}"/> and <see cref="GetHashCode()"/> using <see cref="Id"/> property. Thus, two
    /// arguments with the same id are equal irrespective of other property values. This is done to improve performance
    /// during lookup and avoid multiple arguments with same identifiers.
    /// </para>
    /// <para>
    /// The arguments can have same ids across multiple commands. Each <see cref="Command"/> has
    /// <see cref="ArgumentIdentities"/> collection that contains arguments with unique ids.
    /// </para>
    /// </remarks>
    /// <seealso cref="Command"/>
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
            DataType = argumentIdentity.DataType;
            CustomDataType = argumentIdentity.CustomDataType;
            Description = argumentIdentity.Description;
            Value = value;
            Properties = argumentIdentity.Properties;
        }

        /// <summary>
        /// Initialize a new instance..
        /// </summary>
        /// <param name="id">The argument id.</param>
        /// <param name="value">The argument value.</param>
        /// <param name="customDataType">The argument custom data type.</param>
        public Argument(string id, object value, string customDataType)
        {
            Id = id;
            Value = value;
            DataType = DataType.Custom;
            CustomDataType = customDataType;
        }

        /// <summary>
        /// Initialize a new instance..
        /// </summary>
        /// <param name="id">The argument id.</param>
        /// <param name="value">The argument value.</param>
        /// <param name="dataType">The argument data type.</param>
        public Argument(string id, object value, DataType dataType)
        {
            Id = id;
            Value = value;
            DataType = dataType;
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
        /// The argument id.
        /// </summary>
        /// <remarks>The argument id is unique with in a command.</remarks>
        [JsonPropertyName("id")]
        public string Id { get; set; }

        /// <summary>
        /// The argument custom properties.
        /// </summary>
        [JsonPropertyName("properties")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public Dictionary<string, object>? Properties { get; set; }

        /// <summary>
        /// The argument value.
        /// </summary>
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
            return EqualityComparer<Argument?>.Default.Equals(left, right);
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
                   Id == other.Id;
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
    }
}
