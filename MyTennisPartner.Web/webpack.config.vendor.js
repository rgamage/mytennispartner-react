const path = require('path');
const webpack = require('webpack');
//const ExtractTextPlugin = require('extract-text-webpack-plugin');
const MiniCssExtractPlugin = require("mini-css-extract-plugin");

module.exports = (env, argv) => {
    const extractCSS = new MiniCssExtractPlugin('vendor.css');
    const isDevBuild = !(argv && argv.mode === 'production');
    return [{
        stats: { modules: false },
        resolve: {
            extensions: [ '.js' ]
        },
        module: {
            rules: [
                { test: /\.(png|woff|woff2|eot|ttf|svg)(\?|$)/, use: 'url-loader?limit=100000' },
                //{ test: /\.css(\?|$)/, use: extractCSS.extract([ isDevBuild ? 'css-loader' : 'css-loader?minimize' ]) }
                {
                    test: /\.css$/, use: [
                        MiniCssExtractPlugin.loader,
                        { loader: 'css-loader', options: { url: false, sourceMap: true } }
                    ]
                }
            ]
        },
        entry: {
            vendor: [
                'jquery',
                'popper.js',
                'bootstrap',
                'bootstrap/dist/css/bootstrap.css',
                'es6-promise',
                'event-source-polyfill',
                'isomorphic-fetch',
                'react',
                'react-dom',
                'react-router-dom',
                'url-search-params-polyfill'
            ]
        },
        output: {
            path: path.join(__dirname, 'wwwroot', 'dist'),
            publicPath: 'dist/',
            filename: '[name].js',
            library: '[name]_[hash]'
        },
        plugins: [
            extractCSS,
            new webpack.ProvidePlugin({
                $: 'jquery',
                jQuery: 'jquery',
                'window.jQuery': 'jquery',
                Popper: ['popper.js', 'default']
            }),
            new webpack.DllPlugin({
                path: path.join(__dirname, 'wwwroot', 'dist', '[name]-manifest.json'),
                name: '[name]_[hash]'
            }),
            new webpack.DefinePlugin({
                'process.env.NODE_ENV': isDevBuild ? '"development"' : '"production"'
            })
        ]
            //.concat(isDevBuild ? [] : [
            //new webpack.optimize.UglifyJsPlugin()
            //])
    }];
};
