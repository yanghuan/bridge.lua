    // @source INotifyPropertyChanged.js

    Bridge.define("Bridge.INotifyPropertyChanged");

    Bridge.define("Bridge.PropertyChangedEventArgs", {
        constructor: function (propertyName) {
            this.propertyName = propertyName;
        }
    });
