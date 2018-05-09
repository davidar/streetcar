using UnityEngine;
using UnityEngine.UI;
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

    float _elapsedTime;

    ReverseGeocodeResource _resource;

    Geocoder _geocoder;
    
    Vector2d _coordinate;
    
    void Start() {
        _geocoder = MapboxAccess.Instance.Geocoder;
        _resource = new ReverseGeocodeResource(_coordinate);
    }

    void Update() {
        _elapsedTime += Time.deltaTime;
        
        Ray ray = _camera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0.0f));
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit)) {
            _coordinate = hit.point.GetGeoPosition(_map.CenterMercator, _map.WorldRelativeScale);
            //Debug.Log(string.Format("{0:0.0}: {1:0.0000},{2:0.0000}", _elapsedTime, _coordinate.x, _coordinate.y));
            if (_elapsedTime > 3) {
                _elapsedTime = 0;
                _resource.Query = _coordinate;
                _geocoder.Geocode(_resource, HandleGeocoderResponse);
            }
        }
    }
    
    void HandleGeocoderResponse(ReverseGeocodeResponse res) {
        //Debug.Log(JsonConvert.SerializeObject(res, Formatting.Indented, JsonConverters.Converters));
        _output.text = res.Features[0].PlaceName;
        Debug.Log(_output.text);
    }
}
