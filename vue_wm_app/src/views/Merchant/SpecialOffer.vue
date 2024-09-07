<script setup>
import { useStore } from "vuex";
import { ElMessage } from "element-plus";
import { ref, onMounted, watch } from 'vue';  
import { useRouter } from 'vue-router';
import { CreateSpecialOffer, EditSpecialOffer, DeleteSpecialOffer, GetSpecialOffer } from '@/api/merchant';

const store = useStore();
const router = useRouter();
const gobackHome = () => {
    router.push('/merchant-home');
}
const merchant = ref({}); // 初始化商家信息对象
const offers=ref([]);  //购物车物品列表

const editingOfferId = ref(null);  // 控制当前编辑的活动ID
const editedOffer = ref({ minPrice: '', amountRemission: '' });

const showInputFields = ref(false);  // 控制输入框显示的状态
const newOffer = ref({
  minPrice: '',
  amountRemission: ''
});

// 数字输入验证
const validatePositiveInteger = (value) => {
    if (!/^\d+$/.test(value)) {
        ElMessage.error('请输入正整数');
        return '';
    }
    return value;
}

onMounted(async() => {  
    //获取商铺用户信息
    const merchantData = store.state.merchant; 
    if (merchantData) {  
        merchant.value = merchantData;  
    } else {  
        router.push('/login');
    }  
    const merchantId = merchant.value.MerchantId;

    const data = await GetSpecialOffer(merchantId); 
    if (data) {  
        offers.value = data.data; // 假设后端返回的offer数据是一个数组
    }  
})

// 创建offer函数
const addSpecialOffer = async(offer) => {
    const merchantId = merchant.value.MerchantId;
    const minPrice = validatePositiveInteger(newOffer.value.minPrice);
    const amountRemission = validatePositiveInteger(newOffer.value.amountRemission);

    if (!minPrice || !amountRemission) {
        // 如果验证失败，返回
        return;
    }

    // 调用API，将购物车项保存到数据库    
    try {
      const offerData = {
        MerchantId: merchantId,  // 商户ID
        MinPrice: offer.minprice,  // 满多少元
        AmountRemission: offer.remission  // 减多少元
      };

        const response = await CreateSpecialOffer(offerData); // 调用 API 
        ElMessage.success(response.msg); // 显示成功消息  

        // 将新创建的活动加入到 offers 列表中
        offers.value.push(response.data);

        // 清空输入框
        newOffer.value.minPrice = '';
        newOffer.value.amountRemission = '';
        showInputFields.value = false;  // 隐藏输入框

    } catch (error) {  
      if (error.response && error.response.data) {  
        const errorCode = error.response.data.errorCode;  

        if (errorCode === 20000) {  
          ElMessage.error('添加失败');  
        } 
        else {  
          ElMessage.error('发生未知错误');  
        }  
      } 
      else {  
        console.info(error.toString());
        ElMessage.error('网络错误，请重试');  
      }  
    }  
};

const startEditingOffer = (offer) => {
    editingOfferId.value = offer.offerId;
    editedOffer.value.minPrice = offer.minPrice;
    editedOffer.value.amountRemission = offer.amountRemission;
};

const cancelEditingOffer = () => {
    editingOfferId.value = null;
};

// 修改offer函数
const updateSpecialOffer = async(offer) => {
    const merchantId = merchant.value.MerchantId;
    const minPrice = validatePositiveInteger(editedOffer.value.minPrice);
    const amountRemission = validatePositiveInteger(editedOffer.value.amountRemission);

    if (!minPrice || !amountRemission) {
        // 如果验证失败，返回
        return;
    }

    try {
        const offerData = {
            OfferId: offer.offerId,
            MerchantId: merchantId,
            MinPrice: editedOffer.value.minPrice,
            AmountRemission: editedOffer.value.amountRemission
        };

        const response = await EditSpecialOffer(offerData);
        ElMessage.success(response.msg || '满减活动修改成功');

        // 更新 offers 列表中的数据
        const index = offers.value.findIndex(o => o.offerId === offer.offerId);
        if (index !== -1) {
            offers.value[index].minPrice = editedOffer.value.minPrice;
            offers.value[index].amountRemission = editedOffer.value.amountRemission;
        }

        editingOfferId.value = null;

    } catch (error) {  
        if (error.response && error.response.data) {  
            ElMessage.error(error.response.data.msg || '修改失败');  
        } else {  
            ElMessage.error('网络错误，请重试');  
        }  
    }  
};

const deleteSpecialOffer = async(offerId) => {
    try {
        const response = await DeleteSpecialOffer(offerId);
        ElMessage.success(response.msg || '满减活动删除成功');

        // 从 offers 列表中移除
        offers.value = offers.value.filter(o => o.offerId !== offerId);

        // 如果当前活动正在被编辑，取消编辑状态
        if (editingOfferId.value === offerId) {
            editingOfferId.value = null;
        }

    } catch (error) {  
        if (error.response && error.response.data) {  
            ElMessage.error(error.response.data.msg || '删除失败');  
        } else {  
            ElMessage.error('网络错误，请重试');  
        }  
    }  
};

</script>

<template>
    <div class="content">
        <header class="welcome-text">满减活动</header>
        <div class = "offers-container">
            <ul class = "offers-list">
            <li v-for="offer in offers" :key="offer.offerId">
                <div v-if="editingOfferId !== offer.offerId">
                    满{{offer.minPrice}}元&nbsp;&nbsp;减{{offer.amountRemission}}元
                </div>
                <div class ="right-group" v-if="editingOfferId !== offer.offerId">
                    <button @click="startEditingOffer(offer)" class = "edit-btn">修改</button>
                    <button @click="deleteSpecialOffer(offer.offerId)" class = "delete-btn">删除</button>
                </div>
                <div v-if="editingOfferId == offer.offerId">
                    <el-input v-model="editedOffer.minPrice" placeholder="修改满多少元" @input="editedOffer.minPrice = validatePositiveInteger($event)" />
                    <el-input v-model="editedOffer.amountRemission" placeholder="修改减多少元" @input="editedOffer.amountRemission = validatePositiveInteger($event)" />
                    <el-button type="primary" @click="updateSpecialOffer(offer)" class = "save-btn">保存</el-button>
                    <el-button @click="cancelEditingOffer" class = "delete-btn">取消</el-button>
                </div>
            </li>
            </ul>
        </div>

        <button @click="showInputFields = !showInputFields" class= "normal-button">创建满减活动</button>
        <div v-if="showInputFields" class = "edit-container">
            <el-input v-model="newOffer.minPrice" placeholder="请输入满多少元" @input="editedOffer.minPrice = validatePositiveInteger($event)" />
            <el-input v-model="newOffer.amountRemission" placeholder="请输入减多少元" @input="editedOffer.minPrice = validatePositiveInteger($event)" />
            <el-button style = "margin:10px;" type="primary" @click="addSpecialOffer({ minprice: newOffer.minPrice, remission: newOffer.amountRemission })">提交</el-button>
            <el-button @click="showInputFields = false" class = "cancel-btn">取消</el-button>
        </div>

    </div>
</template>

<style scoped lang ="scss">

.welcome-text {
  font-size: 35px;
  margin-left: 15px;
  color: #000000;
  font-weight: bold;
}

.offers-list {
  list-style: none;
  padding: 0;
  
  overflow-y: auto; /* 使订单区域可以滚动 */

  li {
    display: flex;
    align-items: center;
    padding: 10px;
    margin-left: 30px;     // 修改左右外边距
    margin-right:30px;
    margin-bottom: 10px;
    margin-top:10px;
    border: 2px solid #ffee00;
    border-radius: 8px;
    background-color: #f9f9f9;
    box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);

    img {
      width: 50px;
      height: 50px;
      border-radius: 50%;
      margin-right: 15px;
    }

    span {
      margin-right: 15px;
      font-size: 14px;
      color: #333;
    }

    .right-group {
      margin-left: auto; /* 将右侧按钮推到右边 */
      display: flex;
    }

    button {
      margin-left: 5px;
      margin-right: 5px;
      padding: 6px 12px;
      font-size: 14px;
      border: none;
      border-radius: 4px;
      cursor: pointer;
      transition: background-color 0.3s;

      &.edit-btn {
        background-color: #ffcc00;
        color: white;

        &:hover {
          background-color: #ffdd00;
        }
      }

      &.save-btn {
        background-color: #4caf50;
        color: white;
        margin:10px;

        &:hover {
            background-color: #45a049;
        }
      }

      &.delete-btn {
        background-color: #f44336;
        color: white;

        &:hover {
          background-color: #ff5b58;
        }
      }

      &.cancel-btn {
        background-color: #f44336;
        color: white;
        margin:10px;

        &:hover {
          background-color: #ff5b58;
        }
      }
    }
  }
}

/* 隐藏滚动条 */
.offers-list::-webkit-scrollbar {
    width: 12px;
}

/* 滚动条轨道 */
.offers-list::-webkit-scrollbar-track {
    background: #ffd666;
}
/* 滚动条滑块 */
.offers-list::-webkit-scrollbar-thumb {
    background-color: #ffd666;
    border-radius: 10px;
    border: 2px solid #000000;
}

.offers-container{
    background-color: #ffd666;
    border:2px solid black;
    border-radius: 20px;
    padding:5px;
    
    max-height: 320px; /* 设置订单区域的最大高度 */
    min-height: 110px;
    display: flex;
    flex-direction: column;
    overflow-y: auto; /* 使订单区域可以滚动 */
    
    margin-left: 0px;
    margin-right: 40px;
    margin-bottom:10px;
}

/* 编辑区域的容器样式 */
.edit-container {
  padding: 20px;
  max-width: 600px;
  margin: 10px 30px;
  background-color: #f9f9f9;
  border: 2px solid #ffee00;
  border-radius: 8px;
  box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);

  h2 {
    font-size: 24px;
    margin-bottom: 20px;
    color: #333;
  }

  .el-form {
    display: flex;
    flex-direction: column;

    .el-form-item {
      margin-bottom: 15px;
      label {
        font-weight: bold;
        color: #555;
      }

      input {
        width: 100%;
        padding: 8px;
        border: 1px solid #ddd;
        border-radius: 4px;
        font-size: 14px;
        color: #333;
        outline: none;
        box-sizing: border-box; /* 包括内边距和边框 */
        
        &:focus {
          border-color: #4caf50;
          box-shadow: 0 0 4px rgba(76, 175, 80, 0.2);
        }
      }

      input[type="file"] {
        border: none;
        padding: 0;
      }
    }
  }

  button {
    padding: 10px 20px;
    font-size: 14px;
    border: none;
    border-radius: 4px;
    cursor: pointer;
    margin-right: 10px;
    transition: background-color 0.3s, color 0.3s;
    outline: none;

    &.submit-btn {
      background-color: #4caf50;
      color: white;

      &:hover {
        background-color: #45a049;
      }
    }

    &.cancel-btn {
      background-color: #f44336;
      color: white;

      &:hover {
        background-color: #e53935;
      }
    }
  }
}

.normal-button {
    margin-left: 5px;
    margin-right: 5px;
    padding: 6px 12px;
    font-size: 14px;
    border: none;
    border-radius: 4px;
    cursor: pointer;
    transition: background-color 0.3s;
    background-color: #ffcc00;
    color: white;
}


</style>