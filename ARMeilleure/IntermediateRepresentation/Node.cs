using System;
using System.Collections.Generic;

namespace ARMeilleure.IntermediateRepresentation
{
    class Node
    {
        public Operand Destination
        {
            get
            {
                return _destinations.Count != 0 ? GetDestination(0) : null;
            }
            set
            {
                if (value != null)
                {
                    SetDestination(value);
                }
                else
                {
                    _destinations.Clear();
                }
            }
        }

        private List<Operand> _destinations;
        private List<Operand> _sources;
        private bool _clearedDest;

        public int DestinationsCount => _destinations.Count;
        public int SourcesCount => _sources.Count;

        private void Resize(List<Operand> list, int size)
        {
            while (list.Count > size)
            {
                list.RemoveAt(list.Count - 1);
            }
            while (list.Count < size)
            {
                list.Add(null);
            }
        }

        public Node()
        {
            _destinations = new List<Operand>();
            _sources = new List<Operand>();
        }

        public Node(Operand destination, int sourcesCount) : this()
        {
            Destination = destination;

            Resize(_sources, sourcesCount);
        }

        public Node With(Operand destination, int sourcesCount)
        {
            _clearedDest = true;
            _sources.Clear();
            Destination = destination;

            Resize(_sources, sourcesCount);
            return this;
        }

        public Node With(Operand[] destinations, int sourcesCount)
        {
            _clearedDest = true;
            _sources.Clear();
            SetDestinations(destinations ?? throw new ArgumentNullException(nameof(destinations)));

            Resize(_sources, sourcesCount);
            return this;
        }

        public Operand GetDestination(int index)
        {
            return _destinations[index];
        }

        public Operand GetSource(int index)
        {
            return _sources[index];
        }

        public void SetDestination(int index, Operand destination)
        {
            Operand oldOp = _destinations[index];

            if (oldOp != null && oldOp.Kind == OperandKind.LocalVariable && !_clearedDest)
            {
                oldOp.Assignments.Remove(this);
            }

            if (destination != null && destination.Kind == OperandKind.LocalVariable)
            {
                destination.Assignments.Add(this);
            }

            _clearedDest = false;

            _destinations[index] = destination;
        }

        public void SetSource(int index, Operand source)
        {
            Operand oldOp = _sources[index];

            if (oldOp != null && oldOp.Kind == OperandKind.LocalVariable)
            {
                oldOp.Uses.Remove(this);
            }

            if (source != null && source.Kind == OperandKind.LocalVariable)
            {
                source.Uses.Add(this);
            }

            _sources[index] = source;
        }

        private void RemoveOldDestinations()
        {
            if (_destinations != null && !_clearedDest)
            {
                for (int index = 0; index < _destinations.Count; index++)
                {
                    Operand oldOp = _destinations[index];

                    if (oldOp != null && oldOp.Kind == OperandKind.LocalVariable)
                    {
                        oldOp.Assignments.Remove(this);
                    }
                }
            }
            _clearedDest = false;
        }

        public void SetDestination(Operand destination)
        {
            RemoveOldDestinations();

            Resize(_destinations, 1);

            _destinations[0] = destination;

            if (destination.Kind == OperandKind.LocalVariable)
            {
                destination.Assignments.Add(this);
            }
        }

        public void SetDestinations(Operand[] destinations)
        {
            RemoveOldDestinations();

            Resize(_destinations, destinations.Length);

            for (int index = 0; index < destinations.Length; index++)
            {
                Operand newOp = destinations[index];

                _destinations[index] = newOp;

                if (newOp.Kind == OperandKind.LocalVariable)
                {
                    newOp.Assignments.Add(this);
                }
            }
        }

        private void RemoveOldSources()
        {
            for (int index = 0; index < _sources.Count; index++)
            {
                Operand oldOp = _sources[index];

                if (oldOp != null && oldOp.Kind == OperandKind.LocalVariable)
                {
                    oldOp.Uses.Remove(this);
                }
            }
        }

        public void SetSource(Operand source)
        {
            RemoveOldSources();

            Resize(_sources, 1);

            _sources[0] = source;

            if (source.Kind == OperandKind.LocalVariable)
            {
                source.Uses.Add(this);
            }
        }

        public void SetSources(Operand[] sources)
        {
            RemoveOldSources();

            Resize(_sources, sources.Length);

            for (int index = 0; index < sources.Length; index++)
            {
                Operand newOp = sources[index];

                _sources[index] = newOp;

                if (newOp.Kind == OperandKind.LocalVariable)
                {
                    newOp.Uses.Add(this);
                }
            }
        }
    }
}