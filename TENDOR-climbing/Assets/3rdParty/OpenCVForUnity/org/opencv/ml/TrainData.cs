

using OpenCVForUnity.CoreModule;
using OpenCVForUnity.UtilsModule;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace OpenCVForUnity.MlModule
{
    // C++: class TrainData
    /// <summary>
    ///  Class encapsulating training data.
    /// </summary>
    /// <remarks>
    ///  Please note that the class only specifies the interface of training data, but not implementation.
    ///  All the statistical model classes in _ml_ module accepts Ptr&lt;TrainData&gt; as parameter. In other
    ///  words, you can create your own class derived from TrainData and pass smart pointer to the instance
    ///  of this class into StatModel::train.
    ///  
    ///  @sa @ref ml_intro_data
    /// </remarks>
    public class TrainData : DisposableOpenCVObject
    {

        protected override void Dispose(bool disposing)
        {

            try
            {
                if (disposing)
                {
                }
                if (IsEnabledDispose)
                {
                    if (nativeObj != IntPtr.Zero)
                        ml_TrainData_delete(nativeObj);
                    nativeObj = IntPtr.Zero;
                }
            }
            finally
            {
                base.Dispose(disposing);
            }

        }

        protected internal TrainData(IntPtr addr) : base(addr) { }


        public IntPtr getNativeObjAddr() { return nativeObj; }

        // internal usage only
        public static TrainData __fromPtr__(IntPtr addr) { return new TrainData(addr); }

        //
        // C++:  int cv::ml::TrainData::getLayout()
        //

        public int getLayout()
        {
            ThrowIfDisposed();

            return ml_TrainData_getLayout_10(nativeObj);


        }


        //
        // C++:  int cv::ml::TrainData::getNTrainSamples()
        //

        public int getNTrainSamples()
        {
            ThrowIfDisposed();

            return ml_TrainData_getNTrainSamples_10(nativeObj);


        }


        //
        // C++:  int cv::ml::TrainData::getNTestSamples()
        //

        public int getNTestSamples()
        {
            ThrowIfDisposed();

            return ml_TrainData_getNTestSamples_10(nativeObj);


        }


        //
        // C++:  int cv::ml::TrainData::getNSamples()
        //

        public int getNSamples()
        {
            ThrowIfDisposed();

            return ml_TrainData_getNSamples_10(nativeObj);


        }


        //
        // C++:  int cv::ml::TrainData::getNVars()
        //

        public int getNVars()
        {
            ThrowIfDisposed();

            return ml_TrainData_getNVars_10(nativeObj);


        }


        //
        // C++:  int cv::ml::TrainData::getNAllVars()
        //

        public int getNAllVars()
        {
            ThrowIfDisposed();

            return ml_TrainData_getNAllVars_10(nativeObj);


        }


        //
        // C++:  void cv::ml::TrainData::getSample(Mat varIdx, int sidx, float* buf)
        //

        public void getSample(Mat varIdx, int sidx, float buf)
        {
            ThrowIfDisposed();
            if (varIdx != null) varIdx.ThrowIfDisposed();

            ml_TrainData_getSample_10(nativeObj, varIdx.nativeObj, sidx, buf);


        }


        //
        // C++:  Mat cv::ml::TrainData::getSamples()
        //

        public Mat getSamples()
        {
            ThrowIfDisposed();

            return new Mat(DisposableObject.ThrowIfNullIntPtr(ml_TrainData_getSamples_10(nativeObj)));


        }


        //
        // C++:  Mat cv::ml::TrainData::getMissing()
        //

        public Mat getMissing()
        {
            ThrowIfDisposed();

            return new Mat(DisposableObject.ThrowIfNullIntPtr(ml_TrainData_getMissing_10(nativeObj)));


        }


        //
        // C++:  Mat cv::ml::TrainData::getTrainSamples(int layout = ROW_SAMPLE, bool compressSamples = true, bool compressVars = true)
        //

        /// <summary>
        ///  Returns matrix of train samples
        /// </summary>
        /// <param name="layout">
        /// The requested layout. If it's different from the initial one, the matrix is
        ///          transposed. See ml::SampleTypes.
        /// </param>
        /// <param name="compressSamples">
        /// if true, the function returns only the training samples (specified by
        ///          sampleIdx)
        /// </param>
        /// <param name="compressVars">
        /// if true, the function returns the shorter training samples, containing only
        ///          the active variables.
        /// </param>
        /// <remarks>
        ///      In current implementation the function tries to avoid physical data copying and returns the
        ///      matrix stored inside TrainData (unless the transposition or compression is needed).
        /// </remarks>
        public Mat getTrainSamples(int layout, bool compressSamples, bool compressVars)
        {
            ThrowIfDisposed();

            return new Mat(DisposableObject.ThrowIfNullIntPtr(ml_TrainData_getTrainSamples_10(nativeObj, layout, compressSamples, compressVars)));


        }

        /// <summary>
        ///  Returns matrix of train samples
        /// </summary>
        /// <param name="layout">
        /// The requested layout. If it's different from the initial one, the matrix is
        ///          transposed. See ml::SampleTypes.
        /// </param>
        /// <param name="compressSamples">
        /// if true, the function returns only the training samples (specified by
        ///          sampleIdx)
        /// </param>
        /// <param name="compressVars">
        /// if true, the function returns the shorter training samples, containing only
        ///          the active variables.
        /// </param>
        /// <remarks>
        ///      In current implementation the function tries to avoid physical data copying and returns the
        ///      matrix stored inside TrainData (unless the transposition or compression is needed).
        /// </remarks>
        public Mat getTrainSamples(int layout, bool compressSamples)
        {
            ThrowIfDisposed();

            return new Mat(DisposableObject.ThrowIfNullIntPtr(ml_TrainData_getTrainSamples_11(nativeObj, layout, compressSamples)));


        }

        /// <summary>
        ///  Returns matrix of train samples
        /// </summary>
        /// <param name="layout">
        /// The requested layout. If it's different from the initial one, the matrix is
        ///          transposed. See ml::SampleTypes.
        /// </param>
        /// <param name="compressSamples">
        /// if true, the function returns only the training samples (specified by
        ///          sampleIdx)
        /// </param>
        /// <param name="compressVars">
        /// if true, the function returns the shorter training samples, containing only
        ///          the active variables.
        /// </param>
        /// <remarks>
        ///      In current implementation the function tries to avoid physical data copying and returns the
        ///      matrix stored inside TrainData (unless the transposition or compression is needed).
        /// </remarks>
        public Mat getTrainSamples(int layout)
        {
            ThrowIfDisposed();

            return new Mat(DisposableObject.ThrowIfNullIntPtr(ml_TrainData_getTrainSamples_12(nativeObj, layout)));


        }

        /// <summary>
        ///  Returns matrix of train samples
        /// </summary>
        /// <param name="layout">
        /// The requested layout. If it's different from the initial one, the matrix is
        ///          transposed. See ml::SampleTypes.
        /// </param>
        /// <param name="compressSamples">
        /// if true, the function returns only the training samples (specified by
        ///          sampleIdx)
        /// </param>
        /// <param name="compressVars">
        /// if true, the function returns the shorter training samples, containing only
        ///          the active variables.
        /// </param>
        /// <remarks>
        ///      In current implementation the function tries to avoid physical data copying and returns the
        ///      matrix stored inside TrainData (unless the transposition or compression is needed).
        /// </remarks>
        public Mat getTrainSamples()
        {
            ThrowIfDisposed();

            return new Mat(DisposableObject.ThrowIfNullIntPtr(ml_TrainData_getTrainSamples_13(nativeObj)));


        }


        //
        // C++:  Mat cv::ml::TrainData::getTrainResponses()
        //

        /// <summary>
        ///  Returns the vector of responses
        /// </summary>
        /// <remarks>
        ///      The function returns ordered or the original categorical responses. Usually it's used in
        ///      regression algorithms.
        /// </remarks>
        public Mat getTrainResponses()
        {
            ThrowIfDisposed();

            return new Mat(DisposableObject.ThrowIfNullIntPtr(ml_TrainData_getTrainResponses_10(nativeObj)));


        }


        //
        // C++:  Mat cv::ml::TrainData::getTrainNormCatResponses()
        //

        /// <summary>
        ///  Returns the vector of normalized categorical responses
        /// </summary>
        /// <remarks>
        ///      The function returns vector of responses. Each response is integer from `0` to `&lt;number of
        ///      classes&gt;-1`. The actual label value can be retrieved then from the class label vector, see
        ///      TrainData::getClassLabels.
        /// </remarks>
        public Mat getTrainNormCatResponses()
        {
            ThrowIfDisposed();

            return new Mat(DisposableObject.ThrowIfNullIntPtr(ml_TrainData_getTrainNormCatResponses_10(nativeObj)));


        }


        //
        // C++:  Mat cv::ml::TrainData::getTestResponses()
        //

        public Mat getTestResponses()
        {
            ThrowIfDisposed();

            return new Mat(DisposableObject.ThrowIfNullIntPtr(ml_TrainData_getTestResponses_10(nativeObj)));


        }


        //
        // C++:  Mat cv::ml::TrainData::getTestNormCatResponses()
        //

        public Mat getTestNormCatResponses()
        {
            ThrowIfDisposed();

            return new Mat(DisposableObject.ThrowIfNullIntPtr(ml_TrainData_getTestNormCatResponses_10(nativeObj)));


        }


        //
        // C++:  Mat cv::ml::TrainData::getResponses()
        //

        public Mat getResponses()
        {
            ThrowIfDisposed();

            return new Mat(DisposableObject.ThrowIfNullIntPtr(ml_TrainData_getResponses_10(nativeObj)));


        }


        //
        // C++:  Mat cv::ml::TrainData::getNormCatResponses()
        //

        public Mat getNormCatResponses()
        {
            ThrowIfDisposed();

            return new Mat(DisposableObject.ThrowIfNullIntPtr(ml_TrainData_getNormCatResponses_10(nativeObj)));


        }


        //
        // C++:  Mat cv::ml::TrainData::getSampleWeights()
        //

        public Mat getSampleWeights()
        {
            ThrowIfDisposed();

            return new Mat(DisposableObject.ThrowIfNullIntPtr(ml_TrainData_getSampleWeights_10(nativeObj)));


        }


        //
        // C++:  Mat cv::ml::TrainData::getTrainSampleWeights()
        //

        public Mat getTrainSampleWeights()
        {
            ThrowIfDisposed();

            return new Mat(DisposableObject.ThrowIfNullIntPtr(ml_TrainData_getTrainSampleWeights_10(nativeObj)));


        }


        //
        // C++:  Mat cv::ml::TrainData::getTestSampleWeights()
        //

        public Mat getTestSampleWeights()
        {
            ThrowIfDisposed();

            return new Mat(DisposableObject.ThrowIfNullIntPtr(ml_TrainData_getTestSampleWeights_10(nativeObj)));


        }


        //
        // C++:  Mat cv::ml::TrainData::getVarIdx()
        //

        public Mat getVarIdx()
        {
            ThrowIfDisposed();

            return new Mat(DisposableObject.ThrowIfNullIntPtr(ml_TrainData_getVarIdx_10(nativeObj)));


        }


        //
        // C++:  Mat cv::ml::TrainData::getVarType()
        //

        public Mat getVarType()
        {
            ThrowIfDisposed();

            return new Mat(DisposableObject.ThrowIfNullIntPtr(ml_TrainData_getVarType_10(nativeObj)));


        }


        //
        // C++:  Mat cv::ml::TrainData::getVarSymbolFlags()
        //

        public Mat getVarSymbolFlags()
        {
            ThrowIfDisposed();

            return new Mat(DisposableObject.ThrowIfNullIntPtr(ml_TrainData_getVarSymbolFlags_10(nativeObj)));


        }


        //
        // C++:  int cv::ml::TrainData::getResponseType()
        //

        public int getResponseType()
        {
            ThrowIfDisposed();

            return ml_TrainData_getResponseType_10(nativeObj);


        }


        //
        // C++:  Mat cv::ml::TrainData::getTrainSampleIdx()
        //

        public Mat getTrainSampleIdx()
        {
            ThrowIfDisposed();

            return new Mat(DisposableObject.ThrowIfNullIntPtr(ml_TrainData_getTrainSampleIdx_10(nativeObj)));


        }


        //
        // C++:  Mat cv::ml::TrainData::getTestSampleIdx()
        //

        public Mat getTestSampleIdx()
        {
            ThrowIfDisposed();

            return new Mat(DisposableObject.ThrowIfNullIntPtr(ml_TrainData_getTestSampleIdx_10(nativeObj)));


        }


        //
        // C++:  void cv::ml::TrainData::getValues(int vi, Mat sidx, float* values)
        //

        public void getValues(int vi, Mat sidx, float values)
        {
            ThrowIfDisposed();
            if (sidx != null) sidx.ThrowIfDisposed();

            ml_TrainData_getValues_10(nativeObj, vi, sidx.nativeObj, values);


        }


        //
        // C++:  Mat cv::ml::TrainData::getDefaultSubstValues()
        //

        public Mat getDefaultSubstValues()
        {
            ThrowIfDisposed();

            return new Mat(DisposableObject.ThrowIfNullIntPtr(ml_TrainData_getDefaultSubstValues_10(nativeObj)));


        }


        //
        // C++:  int cv::ml::TrainData::getCatCount(int vi)
        //

        public int getCatCount(int vi)
        {
            ThrowIfDisposed();

            return ml_TrainData_getCatCount_10(nativeObj, vi);


        }


        //
        // C++:  Mat cv::ml::TrainData::getClassLabels()
        //

        /// <summary>
        ///  Returns the vector of class labels
        /// </summary>
        /// <remarks>
        ///      The function returns vector of unique labels occurred in the responses.
        /// </remarks>
        public Mat getClassLabels()
        {
            ThrowIfDisposed();

            return new Mat(DisposableObject.ThrowIfNullIntPtr(ml_TrainData_getClassLabels_10(nativeObj)));


        }


        //
        // C++:  Mat cv::ml::TrainData::getCatOfs()
        //

        public Mat getCatOfs()
        {
            ThrowIfDisposed();

            return new Mat(DisposableObject.ThrowIfNullIntPtr(ml_TrainData_getCatOfs_10(nativeObj)));


        }


        //
        // C++:  Mat cv::ml::TrainData::getCatMap()
        //

        public Mat getCatMap()
        {
            ThrowIfDisposed();

            return new Mat(DisposableObject.ThrowIfNullIntPtr(ml_TrainData_getCatMap_10(nativeObj)));


        }


        //
        // C++:  void cv::ml::TrainData::setTrainTestSplit(int count, bool shuffle = true)
        //

        /// <summary>
        ///  Splits the training data into the training and test parts
        ///      @sa TrainData::setTrainTestSplitRatio
        /// </summary>
        public void setTrainTestSplit(int count, bool shuffle)
        {
            ThrowIfDisposed();

            ml_TrainData_setTrainTestSplit_10(nativeObj, count, shuffle);


        }

        /// <summary>
        ///  Splits the training data into the training and test parts
        ///      @sa TrainData::setTrainTestSplitRatio
        /// </summary>
        public void setTrainTestSplit(int count)
        {
            ThrowIfDisposed();

            ml_TrainData_setTrainTestSplit_11(nativeObj, count);


        }


        //
        // C++:  void cv::ml::TrainData::setTrainTestSplitRatio(double ratio, bool shuffle = true)
        //

        /// <summary>
        ///  Splits the training data into the training and test parts
        /// </summary>
        /// <remarks>
        ///      The function selects a subset of specified relative size and then returns it as the training
        ///      set. If the function is not called, all the data is used for training. Please, note that for
        ///      each of TrainData::getTrain\* there is corresponding TrainData::getTest\*, so that the test
        ///      subset can be retrieved and processed as well.
        ///      @sa TrainData::setTrainTestSplit
        /// </remarks>
        public void setTrainTestSplitRatio(double ratio, bool shuffle)
        {
            ThrowIfDisposed();

            ml_TrainData_setTrainTestSplitRatio_10(nativeObj, ratio, shuffle);


        }

        /// <summary>
        ///  Splits the training data into the training and test parts
        /// </summary>
        /// <remarks>
        ///      The function selects a subset of specified relative size and then returns it as the training
        ///      set. If the function is not called, all the data is used for training. Please, note that for
        ///      each of TrainData::getTrain\* there is corresponding TrainData::getTest\*, so that the test
        ///      subset can be retrieved and processed as well.
        ///      @sa TrainData::setTrainTestSplit
        /// </remarks>
        public void setTrainTestSplitRatio(double ratio)
        {
            ThrowIfDisposed();

            ml_TrainData_setTrainTestSplitRatio_11(nativeObj, ratio);


        }


        //
        // C++:  void cv::ml::TrainData::shuffleTrainTest()
        //

        public void shuffleTrainTest()
        {
            ThrowIfDisposed();

            ml_TrainData_shuffleTrainTest_10(nativeObj);


        }


        //
        // C++:  Mat cv::ml::TrainData::getTestSamples()
        //

        /// <summary>
        ///  Returns matrix of test samples
        /// </summary>
        public Mat getTestSamples()
        {
            ThrowIfDisposed();

            return new Mat(DisposableObject.ThrowIfNullIntPtr(ml_TrainData_getTestSamples_10(nativeObj)));


        }


        //
        // C++:  void cv::ml::TrainData::getNames(vector_String names)
        //

        /// <summary>
        ///  Returns vector of symbolic names captured in loadFromCSV()
        /// </summary>
        public void getNames(List<string> names)
        {
            ThrowIfDisposed();
            Mat names_mat = Converters.vector_String_to_Mat(names);
            ml_TrainData_getNames_10(nativeObj, names_mat.nativeObj);


        }


        //
        // C++: static Mat cv::ml::TrainData::getSubVector(Mat vec, Mat idx)
        //

        /// <summary>
        ///  Extract from 1D vector elements specified by passed indexes.
        /// </summary>
        /// <param name="vec">
        /// input vector (supported types: CV_32S, CV_32F, CV_64F)
        /// </param>
        /// <param name="idx">
        /// 1D index vector
        /// </param>
        public static Mat getSubVector(Mat vec, Mat idx)
        {
            if (vec != null) vec.ThrowIfDisposed();
            if (idx != null) idx.ThrowIfDisposed();

            return new Mat(DisposableObject.ThrowIfNullIntPtr(ml_TrainData_getSubVector_10(vec.nativeObj, idx.nativeObj)));


        }


        //
        // C++: static Mat cv::ml::TrainData::getSubMatrix(Mat matrix, Mat idx, int layout)
        //

        /// <summary>
        ///  Extract from matrix rows/cols specified by passed indexes.
        /// </summary>
        /// <param name="matrix">
        /// input matrix (supported types: CV_32S, CV_32F, CV_64F)
        /// </param>
        /// <param name="idx">
        /// 1D index vector
        /// </param>
        /// <param name="layout">
        /// specifies to extract rows (cv::ml::ROW_SAMPLES) or to extract columns (cv::ml::COL_SAMPLES)
        /// </param>
        public static Mat getSubMatrix(Mat matrix, Mat idx, int layout)
        {
            if (matrix != null) matrix.ThrowIfDisposed();
            if (idx != null) idx.ThrowIfDisposed();

            return new Mat(DisposableObject.ThrowIfNullIntPtr(ml_TrainData_getSubMatrix_10(matrix.nativeObj, idx.nativeObj, layout)));


        }


        //
        // C++: static Ptr_TrainData cv::ml::TrainData::create(Mat samples, int layout, Mat responses, Mat varIdx = Mat(), Mat sampleIdx = Mat(), Mat sampleWeights = Mat(), Mat varType = Mat())
        //

        /// <summary>
        ///  Creates training data from in-memory arrays.
        /// </summary>
        /// <param name="samples">
        /// matrix of samples. It should have CV_32F type.
        /// </param>
        /// <param name="layout">
        /// see ml::SampleTypes.
        /// </param>
        /// <param name="responses">
        /// matrix of responses. If the responses are scalar, they should be stored as a
        ///          single row or as a single column. The matrix should have type CV_32F or CV_32S (in the
        ///          former case the responses are considered as ordered by default; in the latter case - as
        ///          categorical)
        /// </param>
        /// <param name="varIdx">
        /// vector specifying which variables to use for training. It can be an integer vector
        ///          (CV_32S) containing 0-based variable indices or byte vector (CV_8U) containing a mask of
        ///          active variables.
        /// </param>
        /// <param name="sampleIdx">
        /// vector specifying which samples to use for training. It can be an integer
        ///          vector (CV_32S) containing 0-based sample indices or byte vector (CV_8U) containing a mask
        ///          of training samples.
        /// </param>
        /// <param name="sampleWeights">
        /// optional vector with weights for each sample. It should have CV_32F type.
        /// </param>
        /// <param name="varType">
        /// optional vector of type CV_8U and size `&lt;number_of_variables_in_samples&gt; +
        ///          &lt;number_of_variables_in_responses&gt;`, containing types of each input and output variable. See
        ///          ml::VariableTypes.
        /// </param>
        public static TrainData create(Mat samples, int layout, Mat responses, Mat varIdx, Mat sampleIdx, Mat sampleWeights, Mat varType)
        {
            if (samples != null) samples.ThrowIfDisposed();
            if (responses != null) responses.ThrowIfDisposed();
            if (varIdx != null) varIdx.ThrowIfDisposed();
            if (sampleIdx != null) sampleIdx.ThrowIfDisposed();
            if (sampleWeights != null) sampleWeights.ThrowIfDisposed();
            if (varType != null) varType.ThrowIfDisposed();

            return TrainData.__fromPtr__(DisposableObject.ThrowIfNullIntPtr(ml_TrainData_create_10(samples.nativeObj, layout, responses.nativeObj, varIdx.nativeObj, sampleIdx.nativeObj, sampleWeights.nativeObj, varType.nativeObj)));


        }

        /// <summary>
        ///  Creates training data from in-memory arrays.
        /// </summary>
        /// <param name="samples">
        /// matrix of samples. It should have CV_32F type.
        /// </param>
        /// <param name="layout">
        /// see ml::SampleTypes.
        /// </param>
        /// <param name="responses">
        /// matrix of responses. If the responses are scalar, they should be stored as a
        ///          single row or as a single column. The matrix should have type CV_32F or CV_32S (in the
        ///          former case the responses are considered as ordered by default; in the latter case - as
        ///          categorical)
        /// </param>
        /// <param name="varIdx">
        /// vector specifying which variables to use for training. It can be an integer vector
        ///          (CV_32S) containing 0-based variable indices or byte vector (CV_8U) containing a mask of
        ///          active variables.
        /// </param>
        /// <param name="sampleIdx">
        /// vector specifying which samples to use for training. It can be an integer
        ///          vector (CV_32S) containing 0-based sample indices or byte vector (CV_8U) containing a mask
        ///          of training samples.
        /// </param>
        /// <param name="sampleWeights">
        /// optional vector with weights for each sample. It should have CV_32F type.
        /// </param>
        /// <param name="varType">
        /// optional vector of type CV_8U and size `&lt;number_of_variables_in_samples&gt; +
        ///          &lt;number_of_variables_in_responses&gt;`, containing types of each input and output variable. See
        ///          ml::VariableTypes.
        /// </param>
        public static TrainData create(Mat samples, int layout, Mat responses, Mat varIdx, Mat sampleIdx, Mat sampleWeights)
        {
            if (samples != null) samples.ThrowIfDisposed();
            if (responses != null) responses.ThrowIfDisposed();
            if (varIdx != null) varIdx.ThrowIfDisposed();
            if (sampleIdx != null) sampleIdx.ThrowIfDisposed();
            if (sampleWeights != null) sampleWeights.ThrowIfDisposed();

            return TrainData.__fromPtr__(DisposableObject.ThrowIfNullIntPtr(ml_TrainData_create_11(samples.nativeObj, layout, responses.nativeObj, varIdx.nativeObj, sampleIdx.nativeObj, sampleWeights.nativeObj)));


        }

        /// <summary>
        ///  Creates training data from in-memory arrays.
        /// </summary>
        /// <param name="samples">
        /// matrix of samples. It should have CV_32F type.
        /// </param>
        /// <param name="layout">
        /// see ml::SampleTypes.
        /// </param>
        /// <param name="responses">
        /// matrix of responses. If the responses are scalar, they should be stored as a
        ///          single row or as a single column. The matrix should have type CV_32F or CV_32S (in the
        ///          former case the responses are considered as ordered by default; in the latter case - as
        ///          categorical)
        /// </param>
        /// <param name="varIdx">
        /// vector specifying which variables to use for training. It can be an integer vector
        ///          (CV_32S) containing 0-based variable indices or byte vector (CV_8U) containing a mask of
        ///          active variables.
        /// </param>
        /// <param name="sampleIdx">
        /// vector specifying which samples to use for training. It can be an integer
        ///          vector (CV_32S) containing 0-based sample indices or byte vector (CV_8U) containing a mask
        ///          of training samples.
        /// </param>
        /// <param name="sampleWeights">
        /// optional vector with weights for each sample. It should have CV_32F type.
        /// </param>
        /// <param name="varType">
        /// optional vector of type CV_8U and size `&lt;number_of_variables_in_samples&gt; +
        ///          &lt;number_of_variables_in_responses&gt;`, containing types of each input and output variable. See
        ///          ml::VariableTypes.
        /// </param>
        public static TrainData create(Mat samples, int layout, Mat responses, Mat varIdx, Mat sampleIdx)
        {
            if (samples != null) samples.ThrowIfDisposed();
            if (responses != null) responses.ThrowIfDisposed();
            if (varIdx != null) varIdx.ThrowIfDisposed();
            if (sampleIdx != null) sampleIdx.ThrowIfDisposed();

            return TrainData.__fromPtr__(DisposableObject.ThrowIfNullIntPtr(ml_TrainData_create_12(samples.nativeObj, layout, responses.nativeObj, varIdx.nativeObj, sampleIdx.nativeObj)));


        }

        /// <summary>
        ///  Creates training data from in-memory arrays.
        /// </summary>
        /// <param name="samples">
        /// matrix of samples. It should have CV_32F type.
        /// </param>
        /// <param name="layout">
        /// see ml::SampleTypes.
        /// </param>
        /// <param name="responses">
        /// matrix of responses. If the responses are scalar, they should be stored as a
        ///          single row or as a single column. The matrix should have type CV_32F or CV_32S (in the
        ///          former case the responses are considered as ordered by default; in the latter case - as
        ///          categorical)
        /// </param>
        /// <param name="varIdx">
        /// vector specifying which variables to use for training. It can be an integer vector
        ///          (CV_32S) containing 0-based variable indices or byte vector (CV_8U) containing a mask of
        ///          active variables.
        /// </param>
        /// <param name="sampleIdx">
        /// vector specifying which samples to use for training. It can be an integer
        ///          vector (CV_32S) containing 0-based sample indices or byte vector (CV_8U) containing a mask
        ///          of training samples.
        /// </param>
        /// <param name="sampleWeights">
        /// optional vector with weights for each sample. It should have CV_32F type.
        /// </param>
        /// <param name="varType">
        /// optional vector of type CV_8U and size `&lt;number_of_variables_in_samples&gt; +
        ///          &lt;number_of_variables_in_responses&gt;`, containing types of each input and output variable. See
        ///          ml::VariableTypes.
        /// </param>
        public static TrainData create(Mat samples, int layout, Mat responses, Mat varIdx)
        {
            if (samples != null) samples.ThrowIfDisposed();
            if (responses != null) responses.ThrowIfDisposed();
            if (varIdx != null) varIdx.ThrowIfDisposed();

            return TrainData.__fromPtr__(DisposableObject.ThrowIfNullIntPtr(ml_TrainData_create_13(samples.nativeObj, layout, responses.nativeObj, varIdx.nativeObj)));


        }

        /// <summary>
        ///  Creates training data from in-memory arrays.
        /// </summary>
        /// <param name="samples">
        /// matrix of samples. It should have CV_32F type.
        /// </param>
        /// <param name="layout">
        /// see ml::SampleTypes.
        /// </param>
        /// <param name="responses">
        /// matrix of responses. If the responses are scalar, they should be stored as a
        ///          single row or as a single column. The matrix should have type CV_32F or CV_32S (in the
        ///          former case the responses are considered as ordered by default; in the latter case - as
        ///          categorical)
        /// </param>
        /// <param name="varIdx">
        /// vector specifying which variables to use for training. It can be an integer vector
        ///          (CV_32S) containing 0-based variable indices or byte vector (CV_8U) containing a mask of
        ///          active variables.
        /// </param>
        /// <param name="sampleIdx">
        /// vector specifying which samples to use for training. It can be an integer
        ///          vector (CV_32S) containing 0-based sample indices or byte vector (CV_8U) containing a mask
        ///          of training samples.
        /// </param>
        /// <param name="sampleWeights">
        /// optional vector with weights for each sample. It should have CV_32F type.
        /// </param>
        /// <param name="varType">
        /// optional vector of type CV_8U and size `&lt;number_of_variables_in_samples&gt; +
        ///          &lt;number_of_variables_in_responses&gt;`, containing types of each input and output variable. See
        ///          ml::VariableTypes.
        /// </param>
        public static TrainData create(Mat samples, int layout, Mat responses)
        {
            if (samples != null) samples.ThrowIfDisposed();
            if (responses != null) responses.ThrowIfDisposed();

            return TrainData.__fromPtr__(DisposableObject.ThrowIfNullIntPtr(ml_TrainData_create_14(samples.nativeObj, layout, responses.nativeObj)));


        }


#if (UNITY_IOS || UNITY_WEBGL) && !UNITY_EDITOR
        const string LIBNAME = "__Internal";
#else
        const string LIBNAME = "opencvforunity";
#endif



        // C++:  int cv::ml::TrainData::getLayout()
        [DllImport(LIBNAME)]
        private static extern int ml_TrainData_getLayout_10(IntPtr nativeObj);

        // C++:  int cv::ml::TrainData::getNTrainSamples()
        [DllImport(LIBNAME)]
        private static extern int ml_TrainData_getNTrainSamples_10(IntPtr nativeObj);

        // C++:  int cv::ml::TrainData::getNTestSamples()
        [DllImport(LIBNAME)]
        private static extern int ml_TrainData_getNTestSamples_10(IntPtr nativeObj);

        // C++:  int cv::ml::TrainData::getNSamples()
        [DllImport(LIBNAME)]
        private static extern int ml_TrainData_getNSamples_10(IntPtr nativeObj);

        // C++:  int cv::ml::TrainData::getNVars()
        [DllImport(LIBNAME)]
        private static extern int ml_TrainData_getNVars_10(IntPtr nativeObj);

        // C++:  int cv::ml::TrainData::getNAllVars()
        [DllImport(LIBNAME)]
        private static extern int ml_TrainData_getNAllVars_10(IntPtr nativeObj);

        // C++:  void cv::ml::TrainData::getSample(Mat varIdx, int sidx, float* buf)
        [DllImport(LIBNAME)]
        private static extern void ml_TrainData_getSample_10(IntPtr nativeObj, IntPtr varIdx_nativeObj, int sidx, float buf);

        // C++:  Mat cv::ml::TrainData::getSamples()
        [DllImport(LIBNAME)]
        private static extern IntPtr ml_TrainData_getSamples_10(IntPtr nativeObj);

        // C++:  Mat cv::ml::TrainData::getMissing()
        [DllImport(LIBNAME)]
        private static extern IntPtr ml_TrainData_getMissing_10(IntPtr nativeObj);

        // C++:  Mat cv::ml::TrainData::getTrainSamples(int layout = ROW_SAMPLE, bool compressSamples = true, bool compressVars = true)
        [DllImport(LIBNAME)]
        private static extern IntPtr ml_TrainData_getTrainSamples_10(IntPtr nativeObj, int layout, [MarshalAs(UnmanagedType.U1)] bool compressSamples, [MarshalAs(UnmanagedType.U1)] bool compressVars);
        [DllImport(LIBNAME)]
        private static extern IntPtr ml_TrainData_getTrainSamples_11(IntPtr nativeObj, int layout, [MarshalAs(UnmanagedType.U1)] bool compressSamples);
        [DllImport(LIBNAME)]
        private static extern IntPtr ml_TrainData_getTrainSamples_12(IntPtr nativeObj, int layout);
        [DllImport(LIBNAME)]
        private static extern IntPtr ml_TrainData_getTrainSamples_13(IntPtr nativeObj);

        // C++:  Mat cv::ml::TrainData::getTrainResponses()
        [DllImport(LIBNAME)]
        private static extern IntPtr ml_TrainData_getTrainResponses_10(IntPtr nativeObj);

        // C++:  Mat cv::ml::TrainData::getTrainNormCatResponses()
        [DllImport(LIBNAME)]
        private static extern IntPtr ml_TrainData_getTrainNormCatResponses_10(IntPtr nativeObj);

        // C++:  Mat cv::ml::TrainData::getTestResponses()
        [DllImport(LIBNAME)]
        private static extern IntPtr ml_TrainData_getTestResponses_10(IntPtr nativeObj);

        // C++:  Mat cv::ml::TrainData::getTestNormCatResponses()
        [DllImport(LIBNAME)]
        private static extern IntPtr ml_TrainData_getTestNormCatResponses_10(IntPtr nativeObj);

        // C++:  Mat cv::ml::TrainData::getResponses()
        [DllImport(LIBNAME)]
        private static extern IntPtr ml_TrainData_getResponses_10(IntPtr nativeObj);

        // C++:  Mat cv::ml::TrainData::getNormCatResponses()
        [DllImport(LIBNAME)]
        private static extern IntPtr ml_TrainData_getNormCatResponses_10(IntPtr nativeObj);

        // C++:  Mat cv::ml::TrainData::getSampleWeights()
        [DllImport(LIBNAME)]
        private static extern IntPtr ml_TrainData_getSampleWeights_10(IntPtr nativeObj);

        // C++:  Mat cv::ml::TrainData::getTrainSampleWeights()
        [DllImport(LIBNAME)]
        private static extern IntPtr ml_TrainData_getTrainSampleWeights_10(IntPtr nativeObj);

        // C++:  Mat cv::ml::TrainData::getTestSampleWeights()
        [DllImport(LIBNAME)]
        private static extern IntPtr ml_TrainData_getTestSampleWeights_10(IntPtr nativeObj);

        // C++:  Mat cv::ml::TrainData::getVarIdx()
        [DllImport(LIBNAME)]
        private static extern IntPtr ml_TrainData_getVarIdx_10(IntPtr nativeObj);

        // C++:  Mat cv::ml::TrainData::getVarType()
        [DllImport(LIBNAME)]
        private static extern IntPtr ml_TrainData_getVarType_10(IntPtr nativeObj);

        // C++:  Mat cv::ml::TrainData::getVarSymbolFlags()
        [DllImport(LIBNAME)]
        private static extern IntPtr ml_TrainData_getVarSymbolFlags_10(IntPtr nativeObj);

        // C++:  int cv::ml::TrainData::getResponseType()
        [DllImport(LIBNAME)]
        private static extern int ml_TrainData_getResponseType_10(IntPtr nativeObj);

        // C++:  Mat cv::ml::TrainData::getTrainSampleIdx()
        [DllImport(LIBNAME)]
        private static extern IntPtr ml_TrainData_getTrainSampleIdx_10(IntPtr nativeObj);

        // C++:  Mat cv::ml::TrainData::getTestSampleIdx()
        [DllImport(LIBNAME)]
        private static extern IntPtr ml_TrainData_getTestSampleIdx_10(IntPtr nativeObj);

        // C++:  void cv::ml::TrainData::getValues(int vi, Mat sidx, float* values)
        [DllImport(LIBNAME)]
        private static extern void ml_TrainData_getValues_10(IntPtr nativeObj, int vi, IntPtr sidx_nativeObj, float values);

        // C++:  Mat cv::ml::TrainData::getDefaultSubstValues()
        [DllImport(LIBNAME)]
        private static extern IntPtr ml_TrainData_getDefaultSubstValues_10(IntPtr nativeObj);

        // C++:  int cv::ml::TrainData::getCatCount(int vi)
        [DllImport(LIBNAME)]
        private static extern int ml_TrainData_getCatCount_10(IntPtr nativeObj, int vi);

        // C++:  Mat cv::ml::TrainData::getClassLabels()
        [DllImport(LIBNAME)]
        private static extern IntPtr ml_TrainData_getClassLabels_10(IntPtr nativeObj);

        // C++:  Mat cv::ml::TrainData::getCatOfs()
        [DllImport(LIBNAME)]
        private static extern IntPtr ml_TrainData_getCatOfs_10(IntPtr nativeObj);

        // C++:  Mat cv::ml::TrainData::getCatMap()
        [DllImport(LIBNAME)]
        private static extern IntPtr ml_TrainData_getCatMap_10(IntPtr nativeObj);

        // C++:  void cv::ml::TrainData::setTrainTestSplit(int count, bool shuffle = true)
        [DllImport(LIBNAME)]
        private static extern void ml_TrainData_setTrainTestSplit_10(IntPtr nativeObj, int count, [MarshalAs(UnmanagedType.U1)] bool shuffle);
        [DllImport(LIBNAME)]
        private static extern void ml_TrainData_setTrainTestSplit_11(IntPtr nativeObj, int count);

        // C++:  void cv::ml::TrainData::setTrainTestSplitRatio(double ratio, bool shuffle = true)
        [DllImport(LIBNAME)]
        private static extern void ml_TrainData_setTrainTestSplitRatio_10(IntPtr nativeObj, double ratio, [MarshalAs(UnmanagedType.U1)] bool shuffle);
        [DllImport(LIBNAME)]
        private static extern void ml_TrainData_setTrainTestSplitRatio_11(IntPtr nativeObj, double ratio);

        // C++:  void cv::ml::TrainData::shuffleTrainTest()
        [DllImport(LIBNAME)]
        private static extern void ml_TrainData_shuffleTrainTest_10(IntPtr nativeObj);

        // C++:  Mat cv::ml::TrainData::getTestSamples()
        [DllImport(LIBNAME)]
        private static extern IntPtr ml_TrainData_getTestSamples_10(IntPtr nativeObj);

        // C++:  void cv::ml::TrainData::getNames(vector_String names)
        [DllImport(LIBNAME)]
        private static extern void ml_TrainData_getNames_10(IntPtr nativeObj, IntPtr names_mat_nativeObj);

        // C++: static Mat cv::ml::TrainData::getSubVector(Mat vec, Mat idx)
        [DllImport(LIBNAME)]
        private static extern IntPtr ml_TrainData_getSubVector_10(IntPtr vec_nativeObj, IntPtr idx_nativeObj);

        // C++: static Mat cv::ml::TrainData::getSubMatrix(Mat matrix, Mat idx, int layout)
        [DllImport(LIBNAME)]
        private static extern IntPtr ml_TrainData_getSubMatrix_10(IntPtr matrix_nativeObj, IntPtr idx_nativeObj, int layout);

        // C++: static Ptr_TrainData cv::ml::TrainData::create(Mat samples, int layout, Mat responses, Mat varIdx = Mat(), Mat sampleIdx = Mat(), Mat sampleWeights = Mat(), Mat varType = Mat())
        [DllImport(LIBNAME)]
        private static extern IntPtr ml_TrainData_create_10(IntPtr samples_nativeObj, int layout, IntPtr responses_nativeObj, IntPtr varIdx_nativeObj, IntPtr sampleIdx_nativeObj, IntPtr sampleWeights_nativeObj, IntPtr varType_nativeObj);
        [DllImport(LIBNAME)]
        private static extern IntPtr ml_TrainData_create_11(IntPtr samples_nativeObj, int layout, IntPtr responses_nativeObj, IntPtr varIdx_nativeObj, IntPtr sampleIdx_nativeObj, IntPtr sampleWeights_nativeObj);
        [DllImport(LIBNAME)]
        private static extern IntPtr ml_TrainData_create_12(IntPtr samples_nativeObj, int layout, IntPtr responses_nativeObj, IntPtr varIdx_nativeObj, IntPtr sampleIdx_nativeObj);
        [DllImport(LIBNAME)]
        private static extern IntPtr ml_TrainData_create_13(IntPtr samples_nativeObj, int layout, IntPtr responses_nativeObj, IntPtr varIdx_nativeObj);
        [DllImport(LIBNAME)]
        private static extern IntPtr ml_TrainData_create_14(IntPtr samples_nativeObj, int layout, IntPtr responses_nativeObj);

        // native support for java finalize()
        [DllImport(LIBNAME)]
        private static extern void ml_TrainData_delete(IntPtr nativeObj);

    }
}
