using UnityEngine;
using UnityEngine.UI;
using UnityStandardAssets.Utility;
using Mapbox.Directions;
using Mapbox.Geocoding;
using Mapbox.Json;
using Mapbox.Utils;
using Mapbox.Utils.JsonConverters;
using Mapbox.Unity;
using Mapbox.Unity.Map;
using Mapbox.Unity.Utilities;

public class CurrentLocation : MonoBehaviour {
    [SerializeField]
    Camera _camera;

    [SerializeField]
    AbstractMap _map;

    [SerializeField]
    Text _output;

    [SerializeField]
    GameObject _waypointPrefab;

    [SerializeField]
    Vector2d _destination;

    float _elapsedTime;

    Vector2d _coordinate;

    bool _needDirections;

    void Start() {
        _elapsedTime = 2.8f;
        _needDirections = true;
    }

    void Update() {
        _elapsedTime += Time.deltaTime;
        if (_elapsedTime > 3) {
            _elapsedTime = 0;
            UpdateLocation(true);
            if (_needDirections) {
                _needDirections = false;
                MapboxAccess.Instance.Directions.Query(new DirectionResource(new Vector2d[] {_coordinate, _destination}, RoutingProfile.Driving), HandleDirectionsResponse);
            }
        } else {
            UpdateLocation(false);
        }
    }

    void UpdateLocation(bool geocode) {
        Ray ray = _camera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0.0f));
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit)) {
            _coordinate = hit.point.GetGeoPosition(_map.CenterMercator, _map.WorldRelativeScale);
            Debug.Log(string.Format("{0:0.0}: {1:0.0000},{2:0.0000}", _elapsedTime, _coordinate.x, _coordinate.y));
            if (geocode) MapboxAccess.Instance.Geocoder.Geocode(new ReverseGeocodeResource(_coordinate), HandleGeocoderResponse);
        }
    }

    void HandleDirectionsResponse(DirectionsResponse res) {
        Debug.Log(JsonConvert.SerializeObject(res, Formatting.Indented, JsonConverters.Converters));
        LineRenderer lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.positionCount = res.Routes[0].Geometry.Count;
        WaypointCircuit circuit = GetComponent<WaypointCircuit>();
        circuit.waypointList.items = new Transform[lineRenderer.positionCount];
        for (int i = 0; i < lineRenderer.positionCount; i++) {
            Vector2d waypointCoord = res.Routes[0].Geometry[i];
            Debug.Log(waypointCoord);
            GameObject waypoint = Instantiate(_waypointPrefab, Vector3.zero, Quaternion.identity);
            waypoint.transform.MoveToGeocoordinate(waypointCoord, _map.CenterMercator, _map.WorldRelativeScale);
            circuit.waypointList.items[i] = waypoint.transform;
            lineRenderer.SetPosition(i, waypoint.transform.position + 0.15f * Vector3.up);
        }
    }

    void HandleGeocoderResponse(ReverseGeocodeResponse res) {
        _output.text = res.Features[0].PlaceName;
        Debug.Log(_output.text);
    }
}
