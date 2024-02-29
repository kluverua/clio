var iframeintegration;(()=>{"use strict";var e,i,y={883:(e,i,r)=>{var a={"./RemoteEntry":()=>r.e(431).then(()=>()=>r(431))},s=(n,l)=>(r.R=l,l=r.o(a,n)?a[n]():Promise.resolve().then(()=>{throw new Error('Module "'+n+'" does not exist in container.')}),r.R=void 0,l),f=(n,l)=>{if(r.S){var u="default",c=r.S[u];if(c&&c!==n)throw new Error("Container initialization failed as it has already been initialized with a different share scope");return r.S[u]=n,r.I(u,l)}};r.d(i,{get:()=>s,init:()=>f})}},m={};function t(e){var i=m[e];if(void 0!==i)return i.exports;var r=m[e]={exports:{}};return y[e](r,r.exports,t),r.exports}t.m=y,t.c=m,t.d=(e,i)=>{for(var r in i)t.o(i,r)&&!t.o(e,r)&&Object.defineProperty(e,r,{enumerable:!0,get:i[r]})},t.f={},t.e=e=>Promise.all(Object.keys(t.f).reduce((i,r)=>(t.f[r](e,i),i),[])),t.u=e=>e+".js",t.miniCssF=e=>{},t.g=function(){if("object"==typeof globalThis)return globalThis;try{return this||new Function("return this")()}catch{if("object"==typeof window)return window}}(),t.o=(e,i)=>Object.prototype.hasOwnProperty.call(e,i),e={},i="iframeintegration:",t.l=(r,a,s,f)=>{if(e[r])e[r].push(a);else{var n,l;if(void 0!==s)for(var u=document.getElementsByTagName("script"),c=0;c<u.length;c++){var o=u[c];if(o.getAttribute("src")==r||o.getAttribute("data-webpack")==i+s){n=o;break}}n||(l=!0,(n=document.createElement("script")).type="text/javascript",n.charset="utf-8",n.timeout=120,t.nc&&n.setAttribute("nonce",t.nc),n.setAttribute("data-webpack",i+s),n.src=t.tu(r)),e[r]=[a];var d=(p,g)=>{n.onerror=n.onload=null,clearTimeout(h);var b=e[r];if(delete e[r],n.parentNode&&n.parentNode.removeChild(n),b&&b.forEach(v=>v(g)),p)return p(g)},h=setTimeout(d.bind(null,void 0,{type:"timeout",target:n}),12e4);n.onerror=d.bind(null,n.onerror),n.onload=d.bind(null,n.onload),l&&document.head.appendChild(n)}},t.r=e=>{typeof Symbol<"u"&&Symbol.toStringTag&&Object.defineProperty(e,Symbol.toStringTag,{value:"Module"}),Object.defineProperty(e,"__esModule",{value:!0})},(()=>{t.S={};var e={},i={};t.I=(r,a)=>{a||(a=[]);var s=i[r];if(s||(s=i[r]={}),!(a.indexOf(s)>=0)){if(a.push(s),e[r])return e[r];t.o(t.S,r)||(t.S[r]={});var o=[];return e[r]=o.length?Promise.all(o).then(()=>e[r]=1):1}}})(),(()=>{var e;t.tt=()=>(void 0===e&&(e={createScriptURL:i=>i},typeof trustedTypes<"u"&&trustedTypes.createPolicy&&(e=trustedTypes.createPolicy("angular#bundler",e))),e)})(),t.tu=e=>t.tt().createScriptURL(e),(()=>{var e;t.g.importScripts&&(e=t.g.location+"");var i=t.g.document;if(!e&&i&&(i.currentScript&&(e=i.currentScript.src),!e)){var r=i.getElementsByTagName("script");r.length&&(e=r[r.length-1].src)}if(!e)throw new Error("Automatic publicPath is not supported in this browser");e=e.replace(/#.*$/,"").replace(/\?.*$/,"").replace(/\/[^\/]+$/,"/"),t.p=e})(),(()=>{var e={825:0};t.f.j=(a,s)=>{var f=t.o(e,a)?e[a]:void 0;if(0!==f)if(f)s.push(f[2]);else{var n=new Promise((o,d)=>f=e[a]=[o,d]);s.push(f[2]=n);var l=t.p+t.u(a),u=new Error;t.l(l,o=>{if(t.o(e,a)&&(0!==(f=e[a])&&(e[a]=void 0),f)){var d=o&&("load"===o.type?"missing":o.type),h=o&&o.target&&o.target.src;u.message="Loading chunk "+a+" failed.\n("+d+": "+h+")",u.name="ChunkLoadError",u.type=d,u.request=h,f[1](u)}},"chunk-"+a,a)}};var i=(a,s)=>{var u,c,[f,n,l]=s,o=0;if(f.some(h=>0!==e[h])){for(u in n)t.o(n,u)&&(t.m[u]=n[u]);l&&l(t)}for(a&&a(s);o<f.length;o++)t.o(e,c=f[o])&&e[c]&&e[c][0](),e[c]=0},r=self.webpackChunkiframeintegration=self.webpackChunkiframeintegration||[];r.forEach(i.bind(null,0)),r.push=i.bind(null,r.push.bind(r))})();var w=t(883);iframeintegration=w})();