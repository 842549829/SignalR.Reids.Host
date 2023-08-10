
    // Decompiled with JetBrains decompiler
    // Type: Microsoft.AspNetCore.Internal.MemoryBufferWriter
    // Assembly: Microsoft.AspNetCore.Components.Server, Version=7.0.0.0, Culture=neutral, PublicKeyToken=adb9793829ddae60
    // MVID: A41CE03F-5B27-4F4A-ABD0-03C993C94932
    // Assembly location: C:\Program Files\dotnet\shared\Microsoft.AspNetCore.App\7.0.5\Microsoft.AspNetCore.Components.Server.dll
    // XML documentation location: C:\Program Files\dotnet\packs\Microsoft.AspNetCore.App.Ref\7.0.5\ref\net7.0\Microsoft.AspNetCore.Components.Server.xml

    using System;
    using System.Buffers;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using System.Threading.Tasks;


#nullable enable
    namespace SignalR.Redis
    {
        internal sealed class MemoryBufferWriter : Stream, IBufferWriter<byte>
        {

#nullable disable
            [ThreadStatic]
            private static MemoryBufferWriter _cachedInstance;
            private readonly int _minimumSegmentSize;
            private int _bytesWritten;
            private List<MemoryBufferWriter.CompletedBuffer> _completedSegments;
            private byte[] _currentSegment;
            private int _position;

            public MemoryBufferWriter(int minimumSegmentSize = 4096) => this._minimumSegmentSize = minimumSegmentSize;

            public override long Length => (long)this._bytesWritten;

            public override bool CanRead => false;

            public override bool CanSeek => false;

            public override bool CanWrite => true;

            public override long Position
            {
                get => throw new NotSupportedException();
                set => throw new NotSupportedException();
            }


#nullable enable
            public static MemoryBufferWriter Get()
            {
                MemoryBufferWriter memoryBufferWriter = MemoryBufferWriter._cachedInstance;
                if (memoryBufferWriter == null)
                    memoryBufferWriter = new MemoryBufferWriter();
                else
                    MemoryBufferWriter._cachedInstance = (MemoryBufferWriter)null;
                return memoryBufferWriter;
            }

            public static void Return(MemoryBufferWriter writer)
            {
                MemoryBufferWriter._cachedInstance = writer;
                writer.Reset();
            }

            public void Reset()
            {
                if (this._completedSegments != null)
                {
                    for (int index = 0; index < this._completedSegments.Count; ++index)
                        this._completedSegments[index].Return();
                    this._completedSegments.Clear();
                }
                if (this._currentSegment != null)
                {
                    ArrayPool<byte>.Shared.Return(this._currentSegment);
                    this._currentSegment = (byte[])null;
                }
                this._bytesWritten = 0;
                this._position = 0;
            }

            public void Advance(int count)
            {
                this._bytesWritten += count;
                this._position += count;
            }

            public Memory<byte> GetMemory(int sizeHint = 0)
            {
                this.EnsureCapacity(sizeHint);
                return this._currentSegment.AsMemory<byte>(this._position, this._currentSegment.Length - this._position);
            }

            public Span<byte> GetSpan(int sizeHint = 0)
            {
                this.EnsureCapacity(sizeHint);
                return this._currentSegment.AsSpan<byte>(this._position, this._currentSegment.Length - this._position);
            }

            public void CopyTo(IBufferWriter<byte> destination)
            {
                if (this._completedSegments != null)
                {
                    int count = this._completedSegments.Count;
                    for (int index = 0; index < count; ++index)
                        destination.Write<byte>(this._completedSegments[index].Span);
                }
                destination.Write<byte>((ReadOnlySpan<byte>)this._currentSegment.AsSpan<byte>(0, this._position));
            }

            public override Task CopyToAsync(
              Stream destination,
              int bufferSize,
              CancellationToken cancellationToken)
            {
                return this._completedSegments == null && this._currentSegment != null ? destination.WriteAsync(this._currentSegment, 0, this._position, cancellationToken) : this.CopyToSlowAsync(destination, cancellationToken);
            }

            [MemberNotNull("_currentSegment")]
            private void EnsureCapacity(int sizeHint)
            {
                int? length = this._currentSegment?.Length;
                int position = this._position;
                int valueOrDefault = (length.HasValue ? new int?(length.GetValueOrDefault() - position) : new int?()).GetValueOrDefault();
                if (sizeHint == 0 && valueOrDefault > 0 || sizeHint > 0 && valueOrDefault >= sizeHint)
                    return;
                this.AddSegment(sizeHint);
            }

            [MemberNotNull("_currentSegment")]
            private void AddSegment(int sizeHint = 0)
            {
                if (this._currentSegment != null)
                {
                    if (this._completedSegments == null)
                        this._completedSegments = new List<MemoryBufferWriter.CompletedBuffer>();
                    this._completedSegments.Add(new MemoryBufferWriter.CompletedBuffer(this._currentSegment, this._position));
                }
                this._currentSegment = ArrayPool<byte>.Shared.Rent(Math.Max(this._minimumSegmentSize, sizeHint));
                this._position = 0;
            }


#nullable disable
            private async Task CopyToSlowAsync(Stream destination, CancellationToken cancellationToken)
            {
                if (this._completedSegments != null)
                {
                    int count = this._completedSegments.Count;
                    for (int i = 0; i < count; ++i)
                    {
                        MemoryBufferWriter.CompletedBuffer completedSegment = this._completedSegments[i];
                        await destination.WriteAsync((ReadOnlyMemory<byte>)completedSegment.Buffer.AsMemory<byte>(0, completedSegment.Length), cancellationToken).ConfigureAwait(false);
                    }
                }
                if (this._currentSegment == null)
                    return;
                await destination.WriteAsync((ReadOnlyMemory<byte>)this._currentSegment.AsMemory<byte>(0, this._position), cancellationToken).ConfigureAwait(false);
            }


#nullable enable
            public byte[] ToArray()
            {
                if (this._currentSegment == null)
                    return Array.Empty<byte>();
                byte[] array = new byte[this._bytesWritten];
                int start = 0;
                if (this._completedSegments != null)
                {
                    int count = this._completedSegments.Count;
                    for (int index = 0; index < count; ++index)
                    {
                        MemoryBufferWriter.CompletedBuffer completedSegment = this._completedSegments[index];
                        ReadOnlySpan<byte> span = completedSegment.Span;
                        span.CopyTo(array.AsSpan<byte>(start));
                        int num = start;
                        span = completedSegment.Span;
                        int length = span.Length;
                        start = num + length;
                    }
                }
                this._currentSegment.AsSpan<byte>(0, this._position).CopyTo(array.AsSpan<byte>(start));
                return array;
            }

            public void CopyTo(Span<byte> span)
            {
                if (this._currentSegment == null)
                    return;
                int start = 0;
                if (this._completedSegments != null)
                {
                    int count = this._completedSegments.Count;
                    for (int index = 0; index < count; ++index)
                    {
                        MemoryBufferWriter.CompletedBuffer completedSegment = this._completedSegments[index];
                        ReadOnlySpan<byte> span1 = completedSegment.Span;
                        span1.CopyTo(span.Slice(start));
                        int num = start;
                        span1 = completedSegment.Span;
                        int length = span1.Length;
                        start = num + length;
                    }
                }
                this._currentSegment.AsSpan<byte>(0, this._position).CopyTo(span.Slice(start));
            }

            public override void Flush()
            {
            }

            public override Task FlushAsync(CancellationToken cancellationToken) => Task.CompletedTask;

            public override int Read(byte[] buffer, int offset, int count) => throw new NotSupportedException();

            public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();

            public override void SetLength(long value) => throw new NotSupportedException();

            public override void WriteByte(byte value)
            {
                if (this._currentSegment != null && (uint)this._position < (uint)this._currentSegment.Length)
                {
                    this._currentSegment[this._position] = value;
                }
                else
                {
                    this.AddSegment();
                    this._currentSegment[0] = value;
                }
                ++this._position;
                ++this._bytesWritten;
            }

            public override void Write(byte[] buffer, int offset, int count)
            {
                int position = this._position;
                if (this._currentSegment != null && position < this._currentSegment.Length - count)
                {
                    Buffer.BlockCopy((Array)buffer, offset, (Array)this._currentSegment, position, count);
                    this._position = position + count;
                    this._bytesWritten += count;
                }
                else
                    this.Write<byte>((ReadOnlySpan<byte>)buffer.AsSpan<byte>(offset, count));
            }

            public override void Write(ReadOnlySpan<byte> span)
            {
                if (this._currentSegment != null && span.TryCopyTo(this._currentSegment.AsSpan<byte>(this._position)))
                {
                    this._position += span.Length;
                    this._bytesWritten += span.Length;
                }
                else
                    this.Write<byte>(span);
            }

            public MemoryBufferWriter.WrittenBuffers DetachAndReset()
            {
                if (this._completedSegments == null)
                    this._completedSegments = new List<MemoryBufferWriter.CompletedBuffer>();
                if (this._currentSegment != null)
                    this._completedSegments.Add(new MemoryBufferWriter.CompletedBuffer(this._currentSegment, this._position));
                MemoryBufferWriter.WrittenBuffers writtenBuffers = new MemoryBufferWriter.WrittenBuffers(this._completedSegments, this._bytesWritten);
                this._currentSegment = (byte[])null;
                this._completedSegments = (List<MemoryBufferWriter.CompletedBuffer>)null;
                this._bytesWritten = 0;
                this._position = 0;
                return writtenBuffers;
            }

            protected override void Dispose(bool disposing)
            {
                if (!disposing)
                    return;
                this.Reset();
            }

            /// <summary>
            /// Holds the written segments from a MemoryBufferWriter and is no longer attached to a MemoryBufferWriter.
            /// You are now responsible for calling Dispose on this type to return the memory to the pool.
            /// </summary>
            internal readonly ref struct WrittenBuffers
            {
                public readonly List<MemoryBufferWriter.CompletedBuffer> Segments;
                private readonly int _bytesWritten;

                public WrittenBuffers(List<MemoryBufferWriter.CompletedBuffer> segments, int bytesWritten)
                {
                    this.Segments = segments;
                    this._bytesWritten = bytesWritten;
                }

                public int ByteLength => this._bytesWritten;

                public void Dispose()
                {
                    for (int index = 0; index < this.Segments.Count; ++index)
                        this.Segments[index].Return();
                    this.Segments.Clear();
                }
            }

            /// <summary>
            /// Holds a byte[] from the pool and a size value. Basically a Memory but guaranteed to be backed by an ArrayPool byte[], so that we know we can return it.
            /// </summary>
            internal readonly struct CompletedBuffer
            {
                public byte[] Buffer { get; }

                public int Length { get; }

                public ReadOnlySpan<byte> Span => (ReadOnlySpan<byte>)this.Buffer.AsSpan<byte>(0, this.Length);

                public CompletedBuffer(byte[] buffer, int length)
                {
                    this.Buffer = buffer;
                    this.Length = length;
                }

                public void Return() => ArrayPool<byte>.Shared.Return(this.Buffer);
            }
        }
    }

