using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace NewHorizons.Components
{
    public class BrambleSectorController : MonoBehaviour, ISectorGroup
    {
        private Sector _sector;

        private List<Renderer> _renderers = null;
        private List<TessellatedRenderer> _tessellatedRenderers = null;
        private List<Collider> _colliders = null;
        private List<Light> _lights = null;

        public static bool isPlayerInside = false;
        public static bool isProbeInside = false;
        public static bool isShipInside = false;

        private bool _renderersShown = false;

        public Sector GetSector() => _sector;

        public void SetSector(Sector sector)
        {
            if (_sector != null) _sector.OnSectorOccupantsUpdated -= OnSectorOccupantsUpdated;

            _sector = sector;
            _sector.OnSectorOccupantsUpdated += OnSectorOccupantsUpdated;
        }

        private void OnDestroy()
        {
            if (_sector != null) _sector.OnSectorOccupantsUpdated -= OnSectorOccupantsUpdated;
        }

        private void Start()
        {
            _renderers = gameObject.GetComponentsInChildren<Renderer>().ToList();
            _tessellatedRenderers = gameObject.GetComponentsInChildren<TessellatedRenderer>().ToList();
            _colliders = gameObject.GetComponentsInChildren<Collider>().ToList();
            _lights = gameObject.GetComponentsInChildren<Light>().ToList();

            DisableRenderers();
        }

        private void OnSectorOccupantsUpdated()
        {
            if (_sector.ContainsAnyOccupants(DynamicOccupant.Player | DynamicOccupant.Probe))
            {
                if (!_renderersShown) EnableRenderers();
            }
            else
            {
                if (_renderersShown) DisableRenderers();
            }
        }

        private void EnableRenderers()
        {
            foreach (var renderer in _renderers)
            {
                renderer.forceRenderingOff = false;
            }

            foreach (var tessellatedRenderer in _tessellatedRenderers)
            {
                tessellatedRenderer.enabled = true;
            }

            foreach (var collider in _colliders)
            {
                collider.enabled = true;
            }

            foreach (var light in _lights)
            {
                light.enabled = true;
            }
        }

        private void DisableRenderers()
        {
            foreach (var renderer in _renderers)
            {
                renderer.forceRenderingOff = true;
            }

            foreach (var tessellatedRenderer in _tessellatedRenderers)
            {
                tessellatedRenderer.enabled = false;
            }

            foreach (var collider in _colliders)
            {
                collider.enabled = false;
            }

            foreach (var light in _lights)
            {
                light.enabled = false;
            }
        }
    }
}
