const path = require('path');
const webpack = require('webpack');
//const ExtractTextPlugin = require('extract-text-webpack-plugin');
//const CheckerPlugin = require('awesome-typescript-loader').CheckerPlugin;
const bundleOutputDir = './wwwroot/dist';
const MiniCssExtractPlugin = require("mini-css-extract-plugin");
const FriendlyErrorsWebpackPlugin = require('friendly-errors-webpack-plugin');

module.exports = (env, argv) => {
    //const isDevBuild = !(argv && argv.mode === 'production');
    const isDevBuild = true;  // temporary work-around for failing prod builds, need to resolve
    console.log(`mode = ${argv && argv.mode}`);
    return [{
        stats: { modules: false },
        //mode: 'development',  //tried this in attempt to fix HMR, did not work
        entry: { 'main': './ClientApp/boot.tsx' },
        resolve: { extensions: ['.js', '.jsx', '.ts', '.tsx'] },
        output: {
            path: path.join(__dirname, bundleOutputDir),
            filename: '[name].js',
            publicPath: 'dist/'
        },
         module: {
            rules: [
                // this ts-loader method was working, changed to awesome-typescript-loader below as part of HMR troubleshooting
                { test: /\.tsx?$/, include: /ClientApp/, exclude: /node_modules/, loader: "ts-loader", "options": {"transpileOnly": true } },
                //{
                //    test: /\.tsx?$/,
                //    include: /ClientApp/,
                //    exclude: /node_modules/,
                //    loader: [
                //        {
                //            loader: 'awesome-typescript-loader',
                //            options: {
                //                useCache: true,
                //                useBabel: true,
                //                babelOptions: {
                //                    babelrc: false,
                //                    plugins: ['react-hot-loader/babel'],
                //                }
                //            }
                //        }
                //    ]
                //},
                {
                    test: /\.css$/, use: isDevBuild ? ['style-loader', 'css-loader'] :
                        [ MiniCssExtractPlugin.loader,
                            { loader: 'css-loader', options: { url: false, sourceMap: true } }
                        ]
                },
                {
                    test: /\.(sass|scss)$/, use: isDevBuild ? ['style-loader', 'css-loader', 'sass-loader'] :
                        [MiniCssExtractPlugin.loader,
                            { loader: 'css-loader', options: { url: false, sourceMap: true } },
                            { loader: 'sass-loader', options: { sourceMap: true } }
                        ]
                },
                //{ test: /\.css$/, use: ['style-loader', 'css-loader'] },
                //{ test: /\.(sass|scss)$/, use: ['style-loader', 'css-loader', 'sass-loader'])},
                //{ test: /\.(png|jpg|jpeg|gif|svg)$/, use: 'url-loader?limit=25000&publicPath=/dist/&name=[hash].[ext]' },
                //{ test: /\.woff(2)?(\?v=[0-9]\.[0-9]\.[0-9])?$/, loader: "url-loader?limit=10000&mimetype=application/font-woff" },
                //{ test: /\.(ttf|eot|svg|woff(2))(\?v=[0-9]\.[0-9]\.[0-9])?$/, loader: "file-loader?publicPath=/dist/&name=[hash].[ext]" }
                {
                    test: /\.(png|jpg|jpeg|gif)$/, use: [{
                        loader: 'file-loader',
                        options: {
                            name: '[hash].[ext]',
                            outputPath: '/'
                        }
                    }]
                },
                {
                    test: /\.(ttf|eot|svg|woff(2)|woff)(\?v=[0-9]\.[0-9]\.[0-9])?$/, use: [{
                        loader: 'file-loader',
                        options: {
                            name: '[hash].[ext]',
                            outputPath: '/'
                        }
                    }]
                }
                //{
                //    test: /\.js$/,
                //    exclude: /node_modules/,
                //    use: "babel-loader"
                //}
            ]
        },
        plugins: [
            new webpack.HotModuleReplacementPlugin(),
            new FriendlyErrorsWebpackPlugin(),
            //new CheckerPlugin(),
            new webpack.DllReferencePlugin({
                context: __dirname,
                manifest: require('./wwwroot/dist/vendor-manifest.json')
            }),
            new webpack.DefinePlugin({
                'process.env.NODE_ENV': isDevBuild ? '"development"' : '"production"'
            })
        ].concat(isDevBuild ? [
            // Plugins that apply in development builds only
            new webpack.SourceMapDevToolPlugin({
                filename: '[file].map', // Remove this line if you prefer inline source maps
                moduleFilenameTemplate: path.relative(bundleOutputDir, '[resourcePath]') // Point sourcemap entries to the original file locations on disk
            })
        ] : [
            // Plugins that apply in production builds only
         //   new webpack.optimize.UglifyJsPlugin(),
            //new ExtractTextPlugin('site.css')
                new MiniCssExtractPlugin({
                    filename: "site.css"
                })
        ])
    }];
};