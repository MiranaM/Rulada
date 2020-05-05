using System;
using System.Collections.Generic;
using System.Linq;
using PianoRoll.Model.Pitch;

namespace PianoRoll.Model
{
    public abstract class RenderNote : Note
    {
        public const int FREE_CONSONANT_LENGTH = 10;
        public const int MIN_FREE_VOWEL_LENGTH = 20;

        public int RenderLength;
        public long RenderPosition;
        public PitchInfo PitchInfo;
        public VibratoInfo VibratoInfo;

        public override int FinalLength => RenderLength;
        public override long FinalPosition => RenderPosition;
        public bool IsExhale;

        public override bool IsRender => true;

        public abstract Note GetEditorNote();

        public RenderNote(Part part) : base(part)
        {

        }

        public override Note GetPrev()
        {
            throw new Exception();
        }

        public override Note GetNext()
        {
            throw new Exception();
        }
    }

    public class RenderNoteParent : RenderNote
    {
        public List<RenderNoteChild> Children = new List<RenderNoteChild>();
        public List<RenderNoteChild> Heads = new List<RenderNoteChild>(); // -CC for first note in frase
        public Note EditorNote;
        public int ChildrenLength;
        public RenderNoteParent NextParent;
        public RenderNoteParent PrevParent;

        public RenderNoteParent(Part part, Note editorNote) : base(part)
        {
            EditorNote = editorNote;
        }

        public void AddChild(RenderNoteChild renderNote)
        {
            Children.Add(renderNote);
        }

        public void AddChildAsFirst(RenderNoteChild renderNote)
        {
            Children.Insert(0, renderNote);
        }

        public void AddHead(RenderNoteChild renderNote)
        {
            Heads.Add(renderNote);
        }

        public void AddHeadAsFirst(RenderNoteChild renderNote)
        {
            Heads.Insert(0, renderNote);
        }

        public void AttachNextParent(RenderNoteParent renderNote)
        {
            NextParent = renderNote;
            renderNote.PrevParent = this;
            var lastChild = Children.LastOrDefault();
            if (lastChild != null)
            {
                ChildrenLength -= lastChild.RenderLength;
                lastChild.RenderLength = FREE_CONSONANT_LENGTH + (int)renderNote.Pre;
                ChildrenLength += lastChild.RenderLength;

                RenderLength -= ChildrenLength;
            }
            else
            {
                RenderLength -= (int)renderNote.Pre;
            }
            if (RenderLength < MIN_FREE_VOWEL_LENGTH)
                throw new Exception();

            RenderLength = Length - ChildrenLength;
            if (RenderLength < MIN_FREE_VOWEL_LENGTH)
                throw new Exception();

            ResolvePositions();
        }

        public void ResolveLengths(Singer singer)
        {
            for (var i = 0; i < Heads.Count; i++)
            {
                var head = Heads[i];
                var next = (RenderNote) Heads.ElementAtOrDefault(i + 1) ?? this;
                head.RenderLength = (int) (FREE_CONSONANT_LENGTH + next.SafeOto.Preutter);
                if (head.RenderLength <= 0)
                    throw new Exception();
            }

            ChildrenLength = 0;
            for (var index = 0; index < Children.Count; index++)
            {
                var child = Children[index];
                var next =  Children.ElementAtOrDefault(index + 1);
                if (next != null)
                {
                    child.RenderLength = (int) (FREE_CONSONANT_LENGTH + next.SafeOto.Preutter);
                    if (child.RenderLength <= 0)
                        throw new Exception();
                }
                else
                {
                    child.RenderLength = (int) singer.GetRestLength(child.SafeOto.Alias);
                }

                ChildrenLength += child.RenderLength;
            }

            RenderPosition = AbsoluteTime;
            RenderLength = Length;

            var headsOffset = 0;
            for (int i = Heads.Count; i > 0; i--)
            {
                var head = Heads[i - 1];
                headsOffset += head.RenderLength;
                head.RenderPosition = RenderPosition - headsOffset;
                if (head.RenderPosition <= 0)
                    throw new Exception();
            }

            ResolvePositions();
        }

        public override Note GetEditorNote()
        {
            return EditorNote;
        }

        private void ResolvePositions()
        {
            var childrenOffset = 0;
            for (int i = 0; i < Children.Count; i++)
            {
                var child = Children[i];
                child.RenderPosition = RenderPosition + RenderLength + childrenOffset;
                if (child.RenderPosition <= 0)
                    throw new Exception();
                childrenOffset += child.RenderLength;
            }
        }
    }

    public class RenderNoteChild : RenderNote
    {
        public RenderNoteParent Parent;

        public RenderNoteChild(Part part, RenderNoteParent parent) : base(part)
        {
            Parent = parent;
        }

        public override Note GetEditorNote()
        {
            return Parent.EditorNote;
        }
    }
}
