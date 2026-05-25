const { WebAssembly } = require('wasm-feature-detect');

Page({
  data: {
    canvas: null,
    ctx: null,
    gameInstance: null
  },

  onReady: function () {
    this.initGame();
  },

  onUnload: function () {
    if (this.gameInstance) {
      this.gameInstance.Quit();
    }
  },

  initGame: function () {
    const query = wx.createSelectorQuery();
    query.select('#gameCanvas')
      .fields({ node: true, size: true })
      .exec((res) => {
        const canvas = res[0].node;
        const ctx = canvas.getContext('2d');
        const dpr = wx.getSystemInfoSync().pixelRatio;
        
        canvas.width = res[0].width * dpr;
        canvas.height = res[0].height * dpr;
        ctx.scale(dpr, dpr);
        
        this.setData({
          canvas: canvas,
          ctx: ctx
        });
        
        this.loadUnityGame(canvas);
      });
  },

  loadUnityGame: function (canvas) {
    const self = this;
    
    wx.showLoading({
      title: '加载中...'
    });

    const loadScript = (src) => {
      return new Promise((resolve, reject) => {
        const script = document.createElement('script');
        script.src = src;
        script.onload = resolve;
        script.onerror = reject;
        document.head.appendChild(script);
      });
    };

    Promise.all([
      loadScript('./Build/UnityLoader.js')
    ]).then(() => {
      self.gameInstance = UnityLoader.instantiate(
        'gameCanvas',
        './Build/wechat.jslib',
        {
          onProgress: (progress) => {
            console.log(`Loading progress: ${progress}%`);
            wx.showLoading({
              title: `加载中 ${Math.round(progress)}%`
            });
          }
        }
      );
      
      self.gameInstance.then(() => {
        wx.hideLoading();
        console.log('Unity game loaded successfully');
      }).catch((error) => {
        wx.hideLoading();
        console.error('Unity game load failed:', error);
        wx.showToast({
          title: '加载失败',
          icon: 'none'
        });
      });
    }).catch((error) => {
      wx.hideLoading();
      console.error('Failed to load UnityLoader:', error);
      wx.showToast({
        title: '加载失败',
        icon: 'none'
      });
    });
  },

  onTouchStart: function (e) {
    if (this.gameInstance && this.gameInstance.SendMessage) {
      const touch = e.touches[0];
      this.gameInstance.SendMessage('GameManager', 'OnTouchStart', JSON.stringify({
        x: touch.clientX,
        y: touch.clientY
      }));
    }
  },

  onTouchMove: function (e) {
    if (this.gameInstance && this.gameInstance.SendMessage) {
      const touch = e.touches[0];
      this.gameInstance.SendMessage('GameManager', 'OnTouchMove', JSON.stringify({
        x: touch.clientX,
        y: touch.clientY
      }));
    }
  },

  onTouchEnd: function (e) {
    if (this.gameInstance && this.gameInstance.SendMessage) {
      this.gameInstance.SendMessage('GameManager', 'OnTouchEnd');
    }
  },

  onTouchCancel: function (e) {
    if (this.gameInstance && this.gameInstance.SendMessage) {
      this.gameInstance.SendMessage('GameManager', 'OnTouchCancel');
    }
  }
});